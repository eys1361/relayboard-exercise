-- RelayBoard schema (SQLite). EF Core creates this via EnsureCreated.
-- Use these tables for TICKET.md and later features (capacity, history, filters).

CREATE TABLE VehicleTypes (
  Id INTEGER PRIMARY KEY,
  Code TEXT NOT NULL UNIQUE,
  Name TEXT NOT NULL
);

CREATE TABLE DriverStatuses (
  Id INTEGER PRIMARY KEY,
  Code TEXT NOT NULL UNIQUE,
  Name TEXT NOT NULL
);

CREATE TABLE OrderStatuses (
  Id INTEGER PRIMARY KEY,
  Code TEXT NOT NULL UNIQUE,
  Name TEXT NOT NULL
);

CREATE TABLE Addresses (
  Id INTEGER PRIMARY KEY,
  Line1 TEXT NOT NULL,
  Line2 TEXT NULL,
  City TEXT NOT NULL,
  State TEXT NOT NULL,
  PostalCode TEXT NOT NULL,
  Latitude REAL NOT NULL,
  Longitude REAL NOT NULL
);

CREATE TABLE Customers (
  Id INTEGER PRIMARY KEY,
  Name TEXT NOT NULL,
  Phone TEXT NULL,
  Email TEXT NULL
);

CREATE TABLE Drivers (
  Id INTEGER PRIMARY KEY,
  FirstName TEXT NOT NULL,
  LastName TEXT NOT NULL,
  Phone TEXT NULL,
  VehicleTypeId INTEGER NOT NULL REFERENCES VehicleTypes (Id),
  DriverStatusId INTEGER NOT NULL REFERENCES DriverStatuses (Id),
  CurrentLatitude REAL NOT NULL,
  CurrentLongitude REAL NOT NULL,
  LastLocationAt TEXT NOT NULL
);

CREATE TABLE Orders (
  Id INTEGER PRIMARY KEY,
  OrderNumber TEXT NOT NULL UNIQUE,
  CustomerId INTEGER NOT NULL REFERENCES Customers (Id),
  PickupAddressId INTEGER NOT NULL REFERENCES Addresses (Id),
  DropoffAddressId INTEGER NOT NULL REFERENCES Addresses (Id),
  OrderStatusId INTEGER NOT NULL REFERENCES OrderStatuses (Id),
  RequiredVehicleTypeId INTEGER NULL REFERENCES VehicleTypes (Id),
  AssignedDriverId INTEGER NULL REFERENCES Drivers (Id),
  ReadyAt TEXT NOT NULL,
  PickupBy TEXT NOT NULL,
  DeliverBy TEXT NOT NULL,
  Notes TEXT NULL,
  CreatedAt TEXT NOT NULL
);

CREATE TABLE Assignments (
  Id INTEGER PRIMARY KEY,
  OrderId INTEGER NOT NULL REFERENCES Orders (Id) ON DELETE CASCADE,
  DriverId INTEGER NOT NULL REFERENCES Drivers (Id),
  StopSequence INTEGER NOT NULL,
  AssignedAt TEXT NOT NULL,
  UnassignedAt TEXT NULL
);

CREATE INDEX IX_Drivers_DriverStatusId ON Drivers (DriverStatusId);
CREATE INDEX IX_Drivers_VehicleTypeId ON Drivers (VehicleTypeId);
CREATE INDEX IX_Orders_OrderStatusId ON Orders (OrderStatusId);
CREATE INDEX IX_Orders_AssignedDriverId ON Orders (AssignedDriverId);
CREATE INDEX IX_Assignments_OrderId ON Assignments (OrderId);
CREATE INDEX IX_Assignments_DriverId_UnassignedAt ON Assignments (DriverId, UnassignedAt);
CREATE INDEX IX_Assignments_DriverId_StopSequence ON Assignments (DriverId, StopSequence);
