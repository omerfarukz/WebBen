# WebBen
An experimental http benchmarker sample.

## Configuration
```
[
  {
    "Name": "TurengSimpleGet^pen!get",
    "Uri": "https://ac.tureng.co/?t=deneme&l=entr",
    "HttpMethod": "GET",
    "NumberOfRequests": 1000,
    "FetchContent": true,
    "Parallelism": 500,
    "BoundedCapacity": 250,
    "UseDefaultCredentials": false,
    "UseCookieContainer": false
  },
  {
    "Name": "TurengSimpleGet^door",
    "Uri": "https://ac.tureng.co/?t=door&=entr",
    "HttpMethod": "GET",
    "NumberOfRequests": 1000,
    "FetchContent": false,
    "Parallelism": 100,
    "BoundedCapacity": 100,
    "UseDefaultCredentials": false,
    "UseCookieContainer": false
  }
]
```

## Output
```
 | Name                    | NoR  | Pll | BC  | Err | Avg    | Min   | Max      | P90    | P80    | Median | 
 |---------------------------------------------------------------------------------------------------------| 
 | TurengSimpleGet^pen!get | 1000 | 500 | 250 | 0   | 345.50 | 25.36 | 1,240.98 | 924.09 | 748.85 | 248.23 | 
 | TurengSimpleGet^door    | 1000 | 100 | 100 | 0   | 85.02  | 24.94 | 740.27   | 293.38 | 68.92  | 41.66  |
```
