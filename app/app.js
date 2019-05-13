var levelup = require('levelup')
var leveldown = require('leveldown')

var db = levelup(leveldown('./history'))

var express = require('express');
var bodyParser     =        require("body-parser");

var app = express();
app.use(bodyParser.urlencoded({ extended: false }));
app.use(bodyParser.json());
app.use(express.static('public'));

const ejs = require('ejs');
const path = require('path');
app.engine('html', ejs.__express);
app.set('views', path.join(__dirname, '/views'));
app.set('view engine', 'html');

app.get('/', function (req, res) {
    res.render('home');
});

app.post('/', async function (req, res) {
    var data = await db.get('history');
    var histories = JSON.parse(data);
    histories.unshift({
        date: req.body.date,
        flightNo: req.body.flight_no,
        departure: req.body.departure,
        destination: req.body.destination,
        departureTime: req.body.departure_time,
        arriveTime: "-",
        arriveTimeActual: "-",
        neoAddress: "AbWLb9muSiTQCUokstuQu9wVxcqSrSQUnU",
        amount: req.body.amount,
        payout: "-",
    })
    var result = await db.put('history', JSON.stringify(histories));
    if(result) {
        console.log("error:", result)
        res.send({"msg": "error"});
    } else {
        console.log("add policy success")
        res.send({"msg": "success"});
    }
});

app.get('/history', async function (req, res) {
    var data = await db.get('history');
    var histories = JSON.parse(data);
    res.render('his', {histories: histories});
});

app.get('/my', async function (req, res) {
    var data = await db.get('history');
    var json_data = JSON.parse(data);
    var addr = "AbWLb9muSiTQCUokstuQu9wVxcqSrSQUnU";
    var histories = [];
    for(var i=0;i<json_data.length;i++){
        if(json_data[i].neoAddress == addr) {
            histories.push(json_data[i]);
        }
    }
    res.render('his', {histories: histories});
});

app.listen(3000, function () {
  console.log('app listening on port 3000!');
});
