# Home Appliance Repair Center (C# OOP Project)

Course: Object-Oriented Programming & Design - TIC_IPG203_F25

Console application that manages appliances (refrigerators, washing machines, air conditioners),
their repair status and repair cost.

## Run
    dotnet run

## Structure
    Interfaces/  IRepairable
    Models/      Appliance (abstract), Refrigerator, WashingMachine, AirConditioner
    Enums/       RepairStatus
    Events/      Delegates (StatusChangedHandler, DeliveryDelayedHandler)
    Services/    RepairCenter
    Helpers/     Validator (static class)
    Program.cs   Console menu

## OOP principles
Abstraction, Inheritance & Polymorphism, Encapsulation, Delegates & Events, Static members.
