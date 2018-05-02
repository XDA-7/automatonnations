import Vue from "vue";

var HexValues = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F']
var SystemColorMode = {
    empire: 1,
    development: 2
}

Vue.component('tick-picker',
{
    props: ['tick'],
    template: '<span><button v-on:click="$emit(\'increment-tick\')">+</button><span>{{tick}}</span><button v-on:click="$emit(\'decrement-tick\')">-</button></span>'
})

Vue.component('system-display-picker',
{
    template: '<span><button v-on:click="$emit(\'display-empire-color\')">Empire</button><button v-on:click="$emit(\'display-development-color\')">Development</button></span>'
})

function randomHexValue() {
    return HexValues[Math.floor(Math.random() * 16)]
}
function randomColor() {
    return '#' + randomHexValue() + randomHexValue() + randomHexValue() + randomHexValue() + randomHexValue() + randomHexValue()
}
function initEmpires(self) {
    var firstSectorView = self.sectorViews[0]
    var systems = firstSectorView.StarSystems
    for (var i = 0; i < systems.length; i++) {
        var empireId = systems[i].EmpireIds[0]
        if (!self.empireColorMap[empireId]) {
            self.empireColorMap[empireId] = randomColor()
        }
    }
}

var vue = new Vue({
    el: '#demo',
    data: {
        simulationTick: 1,
        sectorViews: [],
        empireColorMap: {},
        selectedEmpireEntry: '',
        selectedEmpire: undefined,
        systemColorMode: SystemColorMode.empire,
        maxDevelopment: 0
    },
    mounted: function() {
        var self = this
        var requester = new XMLHttpRequest()
        requester.open('get', 'http://localhost:3000/simulation/5ae9af4ed7a95b23a49cbd36/general')
        requester.onload = function() {
            var response = JSON.parse(this.response)
            for (var i = 0; i < response.length; i++) {
                self.sectorViews.push(response[i])
            }

            initEmpires(self)
            console.log(JSON.parse(JSON.stringify(self.empireColorMap)))
        }
        requester.send()
    },
    computed: {
        fullName: function() {
            return this.firstName + ' ' + this.lastName
        },
        starSystems : function() {
            if (!this.sectorViews || this.sectorViews.length == 0) {
                return result
            }
            if (this.simulationTick >= this.sectorViews.length) {
                this.simulationTick = this.sectorViews.length - 1
            }
            
            var result = []
            var systems = this.sectorViews[this.simulationTick].StarSystems
            for(var i = 0; i < systems.length; i++) {
                var system = systems[i]
                result.push({
                    x: system.Coordinate.X,
                    y: system.Coordinate.Y,
                    development: system.Development,
                    empireId: system.EmpireIds[0],
                    empireColor: this.empireColorMap[system.EmpireIds[0]]
                })
            }

            this.maxDevelopment = result.reduce(function(current, next) {
                return Math.max(current, next.development)
            },
            0)

            return result
        },
        systemConnections: function() {
            if (!this.sectorViews || this.sectorViews.length == 0) {
                return result
            }
            if (this.simulationTick >= this.sectorViews.length) {
                this.simulationTick = this.sectorViews.length - 1
            }

            var result = []
            var connections = this.sectorViews[this.simulationTick].StarSystemConnections
            for (var i = 0; i < connections.length; i++) {
                var connection = connections[i]
                result.push({
                    x1: connection.Source.X,
                    y1: connection.Source.Y,
                    x2: connection.Destination.X,
                    y2: connection.Destination.Y
                })
            }

            return result
        }
    },
    methods: {
        normalisedPos: function(pos) {
            return pos * (1000 / 20) + 50
        },
        displayColor(system) {
            if (this.systemColorMode == SystemColorMode.empire) {
                return system.empireColor
            }
            else if (this.systemColorMode == SystemColorMode.development) {
                var developmentRatio = system.development / this.maxDevelopment
                var redVal = Math.floor(developmentRatio * 256)
                var bigEnd = Math.floor(redVal / 16)
                var littleEnd = redVal % 16
                return '#' + HexValues[bigEnd] + HexValues[littleEnd] + '0000'
            }
            else {
                console.log('error, unknown system color mode')
            }
        },
        onIncrementTick: function() {
            this.simulationTick = this.simulationTick + 1
        },
        onDecrementTick: function() {
            this.simulationTick = this.simulationTick - 1
        },
        onSelectEmpire() {
            console.log(JSON.parse(JSON.stringify(this.empireColorMap)))
            if (this.empireColorMap[this.selectedEmpireEntry]) {
                console.log('found')
                this.empireColorMap[this.selectedEmpire] = randomColor()
                this.selectedEmpire = this.selectedEmpireEntry
                this.empireColorMap[this.selectedEmpire] = '#000000'
            }
            else if (this.selectedEmpire) {
                console.log('not found')
                this.empireColorMap[this.selectedEmpire] = randomColor()
                this.selectedEmpire = undefined
            }
            else {
                return
            }

            for (var i = 0; i < this.starSystems.length; i++) {
                this.starSystems[i].empireColor = this.empireColorMap[this.starSystems[i].empireId]
            }
        },
        displayEmpireColor() {
            this.systemColorMode = SystemColorMode.empire
        },
        displayDevelopmentColor() {
            this.systemColorMode = SystemColorMode.development
        }
    }
});
