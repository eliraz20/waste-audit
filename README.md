# Waste Audit

Compare landfill mass and diversion across dated baseline and pilot audits at each site.

## Try it

From this folder with the .NET 10 SDK installed:

```sh
dotnet run -- pilot study.tsv
```

Rows are date, site, phase (baseline or pilot), landfill kg, recycling kg, compost kg and rejected kg from the two diversion streams. Rejected material is counted as landfill once. Shows raw and sample-mass-adjusted change.

The included study.tsv uses tab-separated fields. Comment lines start with `#`.

The original exercise remains available with `dotnet run -- example.tsv`.

## When to use it

A student sustainability team wants to choose which building to target for a waste-sorting pilot.

Example result:

```text
Hall 2026-01-01 baseline: 80/100 kg to landfill
Hall 2026-02-01 pilot: 40/50 kg to landfill
Hall: raw landfill change -40 kg; same-mass change 0 kg at 50 kg sampled; diversion change 0 percentage points
```

Related course or field workflow: [University of Regina student campus audit](https://www.uregina.ca/sustainability/waste-audit-2024.html).
