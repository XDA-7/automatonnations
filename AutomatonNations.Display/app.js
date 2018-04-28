/**
 * @typedef {Object} Sector
 * @property {Array.<ObjectId>} StarSystemIds
 * 
 * @typedef {Object} Simulation
 * @property {number} Ticks
 * @property {Array.<ObjectId>} EmpireIds
 * @property {Array.<ObjectId>} WarIds
 * @property {Object} SectorId
 * 
 * @typedef {Object} Empire
 * @property {Array.<ObjectId>} StarSystemIds
 * @property {Object} Alignment
 * @property {number} Military
 * 
 * @typedef {Object} StarSystem
 * @property {Object} Coordinate
 * @property {number} Development
 * @property {Array.<ObjectId>} ConnectedSystemsIds
 */

const Express = require('express')
const app = Express()

const MongoClient = require('mongodb').MongoClient
const ObjectId = require('mongodb').ObjectId
const Db = require('mongodb').Db
const url = 'mongodb://localhost:27017'
const dbname = 'AutomatonNations'


/** @type {Db} */
var db = null

MongoClient.connect(url, function (err, client) {
    if (err) {
        console.log(err.message)
    }
    else {
        db = client.db(dbname)
    }
})

/**
 * 
 * @typedef {function(Array.<StarSystem>)} GetSectorSystemsCallback
 */

/**
 * @function
 * @param {Sector} sector
 * @param {GetSectorSystemsCallback} setSystems
 */
function getSectorSystems(sector, setSystems) {
    var starSystemCollection = db.collection('StarSystems')
    starSystemCollection.find({ _id: { $in: sector.StarSystemIds } }).toArray(function(err, docs) {
        if (err) {
            console.log(err.message)
        }
        else {
            setSystems(docs)
        }
    })
}

app.get('/sector/:sectorId/systems', function(req, res) {
    var sectorCollection = db.collection('Sectors')
    sectorCollection.findOne( { _id : new ObjectId(req.params["sectorId"]) }, function(err, doc) {
        if (err) {
            console.log(err.message)
        }
        else if(doc) {
            getSectorSystems(doc, function(systems) {
                res.send(systems)
            })
        }
        else {
            console.log('sector not found')
            res.status(404)
            res.send()
        }
    } )
})

app.get('/simulation/:simId/general', function(req, res) {
    var presentationCollection = db.collection('PresentationSector')
    presentationCollection
        .find( { SimulationId: new ObjectId(req.params["simId"]) } )
        .toArray(function(err, docs) {
            if (err) {
                console.log(err.message)
            }
            else {
                res.send(docs)
            }
        })
})

app.use('/', Express.static(__dirname + '/view'))
app.use('/', Express.static(__dirname + '/dist'))

app.listen(3000, function() { console.log('server started') })