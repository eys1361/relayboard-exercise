# Interviewer notes — delete before sharing with candidates

Planted traps (do not mention):

1. `DriverService.GetNearbyDriversAsync` comment says reuse it for ranking. It is a bounding box, includes `OFF_DUTY`, ignores plan/SLA, and misses Brooklyn van Devin Park. README also says Euclidean; the ticket says haversine + SLA/deviation.
2. API `DriverDto` uses `lat` / `lng`. Angular `DriverView` uses `latitude` / `longitude`. Address DTOs use `latitude` / `longitude`. Plan stops use `latitude` / `longitude`.
3. `DriverSuggestionTests` comments mention `Score`. The ticket's contract is `milesToPickup`, `extraMiles`, `slaSlipMinutes`, and `reason`. Unskip the test when they implement; do not let them add `Score` to “fix” it.
4. Seeded twist data:
   - Ava: idle van next to Times Square (`RB-1001`) — should win on zero SLA slip.
   - Carla: on-job van even closer, but `RB-1005` Brooklyn deliver-by is tight — high slip if she takes `RB-1001`.
   - Farid: nearby off-duty van — must be excluded.
   - Devin: idle van in Brooklyn — long miles, zero slip.
   - Hector: two-stop plan (`RB-1007` in transit, `RB-1009` still to pick up).

Live twist options (minute 55): van orders still showing cars; don’t suggest anyone with 3+ active assignments; bait a generic ML scoring service (declining is a plus); “why is Carla not first, she's closer?”; map markers at 0,0 because they used `latitude` on `DriverDto`.

Hire bar is in the round-2 canvas, not here.
