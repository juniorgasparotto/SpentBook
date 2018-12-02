"use strict";
var Student = /** @class */ (function () {
    function Student(firstName, middleInitial, lastName) {
        this.firstName = firstName;
        this.middleInitial = middleInitial;
        this.lastName = lastName;
        this.fullName = firstName + " " + middleInitial + " " + lastName;
    }
    return Student;
}());
function greeter(person) {
    return "Hello, " + person.firstName + " " + person.lastName;
}
var user = new Student("Jane", "M.", "User");
var user2 = new Student("Jane", "M.", "User");
var user3 = new Student("Jane", "M.", "User");
console.log("Teste");
document.write("Hello World!");
//# sourceMappingURL=test.js.map