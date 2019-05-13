'use strict';
const axios = require('axios');
const IPFS = require('ipfs-mini');

const ipfs = new IPFS({host: 'ipfs.infura.io', port: 5001, protocol: 'https'});

async function add_flight(flightDetail, landingTime, landingTimeActual) {
    try{
        console.log("add =>", flightDetail, landingTime, landingTimeActual);
        let result = await efdi_contract.methods.addFlightInfo(flightDetail, landingTime, landingTimeActual).send({from: OWNER, gas: GAS});
        console.log(result);
    } catch(err){
        console.log("err:", err.message);
    }
}

async function updateFlights() {
    try {
        let today = new Date();
        today.setDate(today.getDate() - 5);
        let dd = String(today.getDate()).padStart(2, '0');
        let mm = String(today.getMonth() + 1).padStart(2, '0'); //January is 0!
        let yyyy = today.getFullYear();
        let curDay = [yyyy, mm, dd].join('-')

        let url = "http://mart.gouchacha.com/flight/flight-info?date=" + curDay;
        console.log("fetch =>" + url);
        const response = await axios.get(url);
        let data = response.data;
        for(let i=0;i<data.length;i++) {
            if(data[i].actual_arrive_time === "暂无信息") {
                continue
            }
            let flight_detail = [data[i].date_start, 
                                data[i].name,
                                data[i].start_place,
                                data[i].arrive_place,
                                data[i].start_time
                              ].join("|");
            let arrive_time = data[i].arrive_time;
            let actual_arrive_time = data[i].actual_arrive_time;
            console.log(flight_detail, arrive_time, actual_arrive_time)
            if(actual_arrive_time > 3600 + arrive_time) {
                console.log("Delay ===>", flight_detail, arrive_time, actual_arrive_time)
            }
            break;
        }
    } catch (error) {
        console.error(error);
    }
  }

async function import_flight() {
    updateFlights();
}

import_flight();

