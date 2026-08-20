using Microsoft.EntityFrameworkCore;
using RelayBoard.Api.Domain;

namespace RelayBoard.Api.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(RelayBoardContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Orders.AnyAsync(cancellationToken))
        {
            return;
        }

        var now = new DateTime(2026, 8, 18, 14, 0, 0, DateTimeKind.Utc);

        var addresses = new[]
        {
            Addr(1, "1515 Broadway", "New York", "NY", "10036", 40.7580, -73.9855),
            Addr(2, "20 W 34th St", "New York", "NY", "10001", 40.7484, -73.9857),
            Addr(3, "45 Rockefeller Plaza", "New York", "NY", "10111", 40.7587, -73.9787),
            Addr(4, "11 Wall St", "New York", "NY", "10005", 40.7074, -74.0113),
            Addr(5, "208 W 13th St", "New York", "NY", "10011", 40.7380, -74.0010),
            Addr(6, "89 E 42nd St", "New York", "NY", "10017", 40.7527, -73.9772),
            Addr(7, "1000 5th Ave", "New York", "NY", "10028", 40.7794, -73.9632),
            Addr(8, "1 MetroTech Center", "Brooklyn", "NY", "11201", 40.6936, -73.9857),
            Addr(9, "200 Park Ave", "New York", "NY", "10166", 40.7536, -73.9764),
            Addr(10, "30 Rockefeller Plaza", "New York", "NY", "10112", 40.7587, -73.9787),
        };
        db.Addresses.AddRange(addresses);

        var customers = new[]
        {
            new Customer { Id = 1, Name = "Northline Labs", Phone = "212-555-0101", Email = "ops@northline.example" },
            new Customer { Id = 2, Name = "Harbor Dental", Phone = "212-555-0102", Email = "front@harbordental.example" },
            new Customer { Id = 3, Name = "Pike Grocery", Phone = "212-555-0103", Email = "dispatch@pike.example" },
            new Customer { Id = 4, Name = "Elm Legal", Phone = "212-555-0104", Email = "mail@elmlegal.example" },
        };
        db.Customers.AddRange(customers);

        var drivers = new[]
        {
            new Driver
            {
                Id = 1,
                FirstName = "Ava",
                LastName = "Chen",
                Phone = "917-555-1001",
                VehicleTypeId = VehicleTypeIds.Van,
                DriverStatusId = DriverStatusIds.Available,
                CurrentLatitude = 40.7589,
                CurrentLongitude = -73.9851,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 2,
                FirstName = "Ben",
                LastName = "Ortiz",
                Phone = "917-555-1002",
                VehicleTypeId = VehicleTypeIds.Car,
                DriverStatusId = DriverStatusIds.Available,
                CurrentLatitude = 40.7582,
                CurrentLongitude = -73.9854,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 3,
                FirstName = "Carla",
                LastName = "Diaz",
                Phone = "917-555-1003",
                VehicleTypeId = VehicleTypeIds.Van,
                DriverStatusId = DriverStatusIds.OnJob,
                CurrentLatitude = 40.7575,
                CurrentLongitude = -73.9860,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 4,
                FirstName = "Devin",
                LastName = "Park",
                Phone = "917-555-1004",
                VehicleTypeId = VehicleTypeIds.Van,
                DriverStatusId = DriverStatusIds.Available,
                CurrentLatitude = 40.6892,
                CurrentLongitude = -73.9857,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 5,
                FirstName = "Elena",
                LastName = "Brooks",
                Phone = "917-555-1005",
                VehicleTypeId = VehicleTypeIds.Car,
                DriverStatusId = DriverStatusIds.Available,
                CurrentLatitude = 40.7003,
                CurrentLongitude = -73.9888,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 6,
                FirstName = "Farid",
                LastName = "Nassar",
                Phone = "917-555-1006",
                VehicleTypeId = VehicleTypeIds.Van,
                DriverStatusId = DriverStatusIds.OffDuty,
                CurrentLatitude = 40.7590,
                CurrentLongitude = -73.9840,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 7,
                FirstName = "Gia",
                LastName = "Romano",
                Phone = "917-555-1007",
                VehicleTypeId = VehicleTypeIds.Bike,
                DriverStatusId = DriverStatusIds.Available,
                CurrentLatitude = 40.7309,
                CurrentLongitude = -73.9973,
                LastLocationAt = now,
            },
            new Driver
            {
                Id = 8,
                FirstName = "Hector",
                LastName = "Singh",
                Phone = "917-555-1008",
                VehicleTypeId = VehicleTypeIds.Car,
                DriverStatusId = DriverStatusIds.OnJob,
                CurrentLatitude = 40.7520,
                CurrentLongitude = -73.9778,
                LastLocationAt = now,
            },
        };
        db.Drivers.AddRange(drivers);

        var orders = new[]
        {
            Order(1, "RB-1001", 1, pickup: 1, dropoff: 4, OrderStatusIds.Open, VehicleTypeIds.Van, null, now, "Needs a van — two totes.", now.AddMinutes(50), now.AddHours(3)),
            Order(2, "RB-1002", 2, pickup: 1, dropoff: 7, OrderStatusIds.Open, null, null, now, "No vehicle constraint.", now.AddMinutes(70), now.AddHours(3)),
            Order(3, "RB-1003", 3, pickup: 5, dropoff: 9, OrderStatusIds.Open, VehicleTypeIds.Van, null, now, "Van required.", now.AddMinutes(80), now.AddHours(4)),
            Order(4, "RB-1004", 4, pickup: 6, dropoff: 2, OrderStatusIds.Open, VehicleTypeIds.Car, null, now, "Sedan is fine.", now.AddMinutes(60), now.AddHours(3)),
            Order(5, "RB-1005", 1, pickup: 3, dropoff: 8, OrderStatusIds.Assigned, null, 3, now.AddMinutes(-40), "Already with Carla.", now.AddMinutes(10), now.AddMinutes(45)),
            Order(6, "RB-1006", 2, pickup: 4, dropoff: 1, OrderStatusIds.Open, VehicleTypeIds.Van, null, now, "Wall Street pickup.", now.AddMinutes(90), now.AddHours(4)),
            Order(7, "RB-1007", 3, pickup: 9, dropoff: 5, OrderStatusIds.InTransit, null, 8, now.AddMinutes(-70), "Hector en route.", now.AddMinutes(-20), now.AddMinutes(40)),
            Order(8, "RB-1008", 4, pickup: 7, dropoff: 10, OrderStatusIds.Open, VehicleTypeIds.Van, null, now, "Upper East Side.", now.AddMinutes(75), now.AddHours(3)),
            Order(9, "RB-1009", 1, pickup: 2, dropoff: 6, OrderStatusIds.Assigned, VehicleTypeIds.Car, 8, now.AddMinutes(-20), "Second active job for Hector.", now.AddMinutes(90), now.AddMinutes(150)),
            Order(10, "RB-1010", 2, pickup: 10, dropoff: 3, OrderStatusIds.Delivered, null, null, now.AddHours(-3), "Done — ignore for dispatch.", now.AddHours(-2), now.AddHours(-1)),
        };
        db.Orders.AddRange(orders);

        db.Assignments.AddRange(
            new Assignment { OrderId = 5, DriverId = 3, AssignedAt = now.AddMinutes(-40), StopSequence = 1 },
            new Assignment { OrderId = 7, DriverId = 8, AssignedAt = now.AddMinutes(-70), StopSequence = 1 },
            new Assignment { OrderId = 9, DriverId = 8, AssignedAt = now.AddMinutes(-20), StopSequence = 2 });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Address Addr(
        int id,
        string line1,
        string city,
        string state,
        string postal,
        double lat,
        double lng) =>
        new()
        {
            Id = id,
            Line1 = line1,
            City = city,
            State = state,
            PostalCode = postal,
            Latitude = lat,
            Longitude = lng,
        };

    private static Order Order(
        int id,
        string number,
        int customerId,
        int pickup,
        int dropoff,
        int statusId,
        int? vehicleTypeId,
        int? driverId,
        DateTime readyAt,
        string notes,
        DateTime pickupBy,
        DateTime deliverBy) =>
        new()
        {
            Id = id,
            OrderNumber = number,
            CustomerId = customerId,
            PickupAddressId = pickup,
            DropoffAddressId = dropoff,
            OrderStatusId = statusId,
            RequiredVehicleTypeId = vehicleTypeId,
            AssignedDriverId = driverId,
            ReadyAt = readyAt,
            PickupBy = pickupBy,
            DeliverBy = deliverBy,
            Notes = notes,
            CreatedAt = readyAt,
        };
}
