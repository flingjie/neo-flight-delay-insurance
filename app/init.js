var levelup = require('levelup')
var leveldown = require('leveldown')

var db = levelup(leveldown('./history'))

var history = JSON.stringify([
  {
      date: "2019-04-23",
      flightNo: "3K831",
      departure: "樟宜T1",
      destination: "徐州观音T1",
      departureTime: "05: 00",
      arriveTime: "10:20",
      arriveTimeActual: "10:11",
      neoAddress: "AT6tNx4yE7htwQgYQbtb7YqshUWznGRyDr",
      amount: "30",
      payout: "40",
  },
  {
      date: "2019-04-22",
      flightNo: "3U3001",
      departure: "广州白云T2",
      destination: "北京首都T2",
      departureTime: "08: 00",
      arriveTime: "11:25",
      arriveTimeActual: "12:49",
      neoAddress: "AbWLb9muSiTQCUokstuQu9wVxcqSrSQUnU",
      amount: "20",
      payout: "-",
  },
  {
      date: "2019-04-21",
      flightNo: "3U3001",
      departure: "广州白云T2",
      destination: "北京首都T2",
      departureTime: "08: 00",
      arriveTime: "11:25",
      arriveTimeActual: "10:55",
      neoAddress: "AGcdLL4ASmCEFCmJ4C9bQAuFDj9C9m8kvb",
      amount: "30",
      payout: "-",
  },
  {
      date: "2019-04-20",
      flightNo: "3U2022",
      departure: "贵阳龙洞堡T1",
      destination: "成都双流T2",
      departureTime: "00: 40",
      arriveTime: "02:10",
      arriveTimeActual: "02:00",
      neoAddress: "AbWLb9muSiTQCUokstuQu9wVxcqSrSQUnU",
      amount: "20",
      payout: "30",
  },
  {
      date: "2019-04-19",
      flightNo: "3U3053",
      departure: "深圳宝安T3",
      destination: "成都双流T2",
      departureTime: "07: 00",
      arriveTime: "09:35",
      arriveTimeActual: "09:48",
      neoAddress: "AGcdLL4ASmCEFCmJ4C9bQAuFDj9C9m8kvb",
      amount: "20",
      payout: "-",
  }
]);



async function init() {
  var res = await db.put('history', history);
  if(res) {
    console.log("err: ", res)
  } else {
    console.log("init successfully!")
  }
}

init();