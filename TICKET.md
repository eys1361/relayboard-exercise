# Suggest drivers, and show the board on a map

As a dispatcher I need two things in this session:

1. When I select an open order, the three best drivers — **closest is not enough**.
2. A map of **open order pickups** and **available drivers**, visually distinct, so I can see the board at a glance.

Using Cursor or Copilot is expected (including the map and the insertion search). You still own the ranking rules, the marker meaning, and a test that proves ranking.

## A. Driver suggestions

Rank by (in order):

1. **Least SLA impact** — after inserting this order into the driver's current plan, existing stops should miss `pickupBy` / `deliverBy` as little as possible. Zero slip is best.
2. **Least deviation** — smallest extra miles vs the driver's current plan (haversine). Idle drivers have an empty plan; extra miles is then just the new trip.
3. **Closest** — miles from the driver's current location to this order's pickup (haversine, 1 decimal). Tie-break, always displayed.

### Acceptance

1. Selecting an open order shows exactly three suggestions, or fewer if fewer drivers qualify.
2. A driver qualifies if status is `AVAILABLE` or `ON_JOB` (not `OFF_DUTY`).
3. If the order has `requiredVehicleType` set (for example `VAN`), only that vehicle type qualifies.
4. Each suggestion shows: driver name, vehicle type, `milesToPickup`, `extraMiles`, `slaSlipMinutes`, and a short reason.
5. Rank by `slaSlipMinutes` ascending, then `extraMiles` ascending, then `milesToPickup` ascending.
6. Current plan is on `GET /api/drivers` as `currentPlan` (ordered stops). Insert pickup, then dropoff after pickup. Do not reorder the driver's existing stops.
7. Travel model (no traffic API): **3 minutes per mile + 8 minutes dwell per stop**, starting from the driver's current lat/lng at "now".
8. Assign from a suggestion using the **existing** `POST /api/orders/{id}/assign` endpoint.
9. Empty state if no driver qualifies.
10. Add or unskip a backend test: a nearby on-job van with a tight SLA must rank behind an idle van when the idle van has zero slip.

## B. Dispatch map

There is no map in the starter UI. Add one to the dispatcher board.

### What to plot

| Thing | Source | Coordinates |
|---|---|---|
| Open order pickup | `GET /api/orders?status=OPEN` → `pickup` | `pickup.latitude`, `pickup.longitude` |
| Selected order dropoff | same order → `dropoff` | `dropoff.latitude`, `dropoff.longitude` |
| Available driver | `GET /api/drivers` where `status === "AVAILABLE"` | `lat`, `lng` |
| Optional: on-job driver | `status === "ON_JOB"` | `lat`, `lng` — muted; not the primary layer |

Do **not** show `OFF_DUTY` drivers. Do **not** plot delivered/cancelled orders.

### Behavior

1. Open pickups and available drivers are **immediately distinguishable** (shape, color, or label — e.g. order number vs first name). Include a legend.
2. Clicking an open pickup selects that order in the list (same as clicking the list).
3. Selecting an order also shows its dropoff and, once suggestions exist, those three drivers.
4. Fit the map to the markers you plotted (NYC seed data is around 40.74, -73.99).
5. OSM/Leaflet (no API key) is enough. Do not add auth, live GPS, or turn-by-turn routing.

### App details that matter

- Addresses use `latitude` / `longitude`. Driver list DTOs use `lat` / `lng`. Mixing them gives silent `undefined` markers.
- `currentPlan[]` stops also use `latitude` / `longitude` if you draw a driver's route.
- Order SLAs (`pickupBy`, `deliverBy`) belong on the selected-order panel, not as map clutter.

## Out of scope

Live GPS, real road routing, traffic, SignalR, new auth, persistence beyond SQLite already in the repo.

Match existing patterns in `RelayBoard.Api` and `RelayBoard.Web`.
