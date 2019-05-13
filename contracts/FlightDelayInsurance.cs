using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.ComponentModel;
using System.Numerics;
using Helper = Neo.SmartContract.Framework.Helper;

namespace Neo.SmartContract
{
    public struct Poilcy 
    {
        public byte[] user;
        public string flightDetail;
        public ulong price;
    }
  
    public struct FlightInfo
    {
        public ulong landingTime;
        public ulong landingTimeActual;
        public bool isDelay;
    }

    public struct PayoutInfo
    {
        public byte[] user;
        public ulong amount;
    }

    public class FlightDelayInsurance : Framework.SmartContract
    {
        public static string Name() => "Flight Delay Insurance";
        public static string Symbol() => "FDI";
        public static readonly byte[] Owner = "AGcdLL4ASmCEFCmJ4C9bQAuFDj9C9m8kvb".ToScriptHash();
        private const uint factor = 90; 
        private const uint delay_time = 3600;


        [DisplayName("NewPolicy")]
        public static event Action<byte[], string, BigInteger> NewPolicy;

        [DisplayName("NewFlight")]
        public static event Action<string, bool> NewFlight;

        [DisplayName("Checked")]
        public static event Action<uint, uint, uint, uint> Checked;

        [DisplayName("AlreadyChecked")]
        public static event Action<uint> AlreadyChecked;


        [DisplayName("Payout")]
        public static event Action<byte[], uint> Payout;

        public static Object Main(string operation, params object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                if (Owner.Length == 20)
                {
                    // if param Owner is script hash
                    return Runtime.CheckWitness(Owner);
                }
                else if (Owner.Length == 33)
                {
                    // if param Owner is public key
                    byte[] signature = operation.AsByteArray();
                    return VerifySignature(signature, Owner);
                }
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                if (operation == "CreateInsurance") return CreateInsurance((uint)args[0], (string)args[1], (ulong)args[2]);
                if (operation == "AddFlightInfo") return AddFlightInfo((string)args[0], (ulong)args[1], (ulong)args[2]);
                if (operation == "checkInsurnce") return checkInsurnce((string)args[0]);
                if (operation == "setFactor") return setFactor((uint)args[0]);
                if (operation == "setDelayTime") return setDelayTime((uint)args[0]);
                if (operation == "name") return Name();
                if (operation == "symbol") return Symbol();
            }
            //you can choice refund or not refund
            byte[] sender = GetSender();
            ulong contribute_value = GetContributeValue();
            if (contribute_value > 0 && sender.Length != 0)
            {
                Refund(sender, contribute_value);
            }
            return false;
        }

        public static bool CreateInsurance(uint day, string flightDetail, ulong price)
        {
            StorageMap policy_map = Storage.CurrentContext.CreateMap("policies");
            Poilcy[] policies = policies.Get(day);
            byte[] user = GetSender();
            Poilcy p = new Poilcy {
                user = user,
                flightDetail = flight,
                price = price
            };
            policies.Add(p);
            policies.put(day, Helper.Serialize(policies));
            NewFlight(flightDetail);
            return true;
        }

        public static bool AddFlightInfo(string flightDetail, ulong landingTime, ulong landingTimeActual)
        {
            bool isDelay = false;
            if(landingTime + delay_time < landingTimeActual) {
                isDelay = true;
            }
            
            FlightInfo info = new FlightInfo {
                landingTime = landingTime,
                landingTimeActual = landingTimeActual,
                isDelay = isDelay
            };
            StorageMap flight_map = Storage.CurrentContext.CreateMap("flight");
            flight_map.put(flightDetail, Helper.Serialize(info));
            NewFlight(flightDetail, isDelay);
            return true;
        }

        public static PayoutInfo[] checkInsurnce(string day)
        {
            StorageMap payout_storage = Storage.CurrentContext.CreateMap("payout");
            StorageMap flight_storage = Storage.CurrentContext.CreateMap("flight");
            StorageMap policy_storage = Storage.CurrentContext.CreateMap("policies");
            PayoutInfo[] payout_list;
            byte[] tmp = flight_Serialize.Get(day);
            if(tmp.Length == 0) 
                return true;
            Poilcy[] policies = Helper.Serialize(tmp);
            ulong total;
            ulong count;
            foreach(Poilcy p in policies) 
            {
                byte[] flight = flight_storage.Get(p.flightDetail);
                if(flight.Length == 0)
                    continue;
                FlightInfo[] info = Helper.Deserialze(flight) as FlightInfo[];
                if(info.isDelay) 
                {
                    total += p.price;
                    count += 1;
                }
            }
            ulong unit = total * factor / (100 * count);

            foreach(Poilcy p in policies) 
            {
                byte[] flight = flight_storage.Get(p.flightDetail);
                if(flight.Length == 0)
                    continue;
                FlightInfo[] info = Helper.Deserialze(flight) as FlightInfo[];
                if(info.isDelay) 
                {
                    ulong pay = p.price * unit;
                    PayoutInfo info = new PayoutInfo
                    {
                        user = p.user,
                        amount = pay
                    };
                    payout_list.Add(info);
                    Payout(p.user, pay);
                }
            }
            payout_storage.Put(day, Helper.Serialize(payout_list));
            return payout_list;
        }

        public static bool setFactor(uint new_factor)
        {
            if (new_factor <= 0) return false;
            byte[] sender = GetSender();
            if(sender != Owner)
                return false;
            
            factor = new_factor;
            return true;
        }

        public static bool setDelayTime(uint t)
        {
            if (t <= 0) return false;
            byte[] sender = GetSender();
            if(sender != Owner)
                return false;
            
            delay_time = t;
            return true;
        }

        // check whether asset is neo and get sender script hash
        private static byte[] GetSender()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] reference = tx.GetReferences();
            // you can choice refund or not refund
            foreach (TransactionOutput output in reference)
            {
                if (output.AssetId == neo_asset_id) return output.ScriptHash;
            }
            return new byte[]{};
        }

        // get smart contract script hash
        private static byte[] GetReceiver()
        {
            return ExecutionEngine.ExecutingScriptHash;
        }

        // get all you contribute neo amount
        private static ulong GetContributeValue()
        {
            Transaction tx = (Transaction)ExecutionEngine.ScriptContainer;
            TransactionOutput[] outputs = tx.GetOutputs();
            ulong value = 0;
            // get the total amount of Neo
            // 获取转入智能合约地址的Neo总量
            foreach (TransactionOutput output in outputs)
            {
                if (output.ScriptHash == GetReceiver() && output.AssetId == neo_asset_id)
                {
                    value += (ulong)output.Value;
                }
            }
            return value;
        }
    }
}