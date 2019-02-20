function firstFunction() {
    //Basic function
}

function secondFunction(input) {
    //Function with parameter
}

// Assigned function
var assignedFunction = function (a, b) { return a * b };

// Using the Function constructor
var myFunction = new Function("a", "b", "return a * b");

// Arrow functions - ES6
const x = (x, y) => x * y;

// Inner functions
function outerFunction() {
    function innerFunction() {

    }

    var assignedInnerFunction = function (a, b) { return a * b };
}

async function asyncFunction() {
    // Async function
}