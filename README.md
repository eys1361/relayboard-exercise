# RelayBoard

Small same-day courier dispatch console used for a pairing interview. Angular UI + ASP.NET Core API + SQLite.

This is **not** a production system. It is a complete, runnable slice: list orders, list drivers, assign a driver.

## Prerequisites

- .NET 10 SDK
- Node.js 20+

## Run (about 10 minutes)

Terminal 1 — API (http://localhost:5120):

```bash
cd RelayBoard.Api
dotnet run
```

SQLite file `relayboard.db` is created and seeded on first start.

Terminal 2 — web (http://localhost:4200):

```bash
cd RelayBoard.Web
npm install
npm start
```

Open http://localhost:4200. Select an open order, then Assign on a driver.

Useful API checks:

- http://localhost:5120/openapi/v1.json
- `GET /api/orders?status=OPEN`
- `GET /api/drivers`
- `GET /api/lookups`
- `POST /api/orders/{id}/assign` with `{ "driverId": 1 }`

## Tests

```bash
dotnet test
```

Existing order/driver tests should pass. Suggestion tests are skipped until you implement `TICKET.md`.

## Layout

| Project | Role |
|---|---|
| `RelayBoard.Api` | ASP.NET Core 10, EF Core, SQLite |
| `RelayBoard.Web` | Angular dispatcher board |
| `RelayBoard.Api.Tests` | HTTP tests against a temp database |

## Database

Tables are created from EF Core on startup (`EnsureCreated`). Schema is in `RelayBoard.Api/Data/RelayBoardContext.cs`.

- **Lookups:** `VehicleTypes`, `DriverStatuses`, `OrderStatuses`
- **Dispatch:** `Customers`, `Addresses`, `Drivers`, `Orders`, `Assignments`

`Assignments` is the history of who was on an order (`UnassignedAt` null = current, `StopSequence` is the driver's job order). Each order has `PickupBy` / `DeliverBy` SLAs. Driver lat/lng is `Drivers.CurrentLatitude` / `CurrentLongitude`. `GET /api/drivers` includes `currentPlan` (ordered pickup/dropoff stops).

When ranking drivers, use Euclidean distance on lat/lng.

## Pairing ticket

See `TICKET.md`. Do not implement it before the live session unless the interviewer asks you to.
