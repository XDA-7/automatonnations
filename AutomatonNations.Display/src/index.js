import Vue from "vue";

Vue.component('color-picker',
{
    props: ['color'],
    template:`
        '<button v-on:click="$emit('pick-color', color)">{{color}}</button>'
    `
})

var vue = new Vue({
    el: '#demo',
    data: {
        firstName: 'Clow',
        lastName: 'Reed',
        fontColor: 'black',
        colorChoices: [ 'black', 'red', 'blue', 'violet' ],
        starSystems: []
    },
    mounted: function() {
        var self = this
        var requester = new XMLHttpRequest()
        requester.open('get', 'http://localhost:3000/sector/5ac1b1a30554ed1a24df1e53/systems')
        requester.onload = function() {
            var response = JSON.parse(this.response)
            for(var i = 0; i < response.length; i++) {
                var system = response[i]
                self.starSystems.push({
                    x: system.Coordinate.X,
                    y: system.Coordinate.Y
                })
            }
        }
        requester.send()
    },
    computed: {
        fullName: function() {
            return this.firstName + ' ' + this.lastName
        }
    },
    methods: {
        onPickColor: function(color) {
            console.log(color)
            this.fontColor = color
        },
        normalisedPos: function(pos) {
            return pos * (1000 / 20)
        }
    }
});

console.log('script run')
