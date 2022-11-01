# WebBen[chmark]

[![nuget](https://img.shields.io/nuget/dt/webben)](https://www.nuget.org/packages/webben) [![SonarCloud](https://github.com/omerfarukz/WebBen/actions/workflows/sonarqube.yml/badge.svg)](https://sonarcloud.io/summary/overall?id=omerfarukz_WebBen) [![codecov](https://codecov.io/gh/omerfarukz/WebBen/branch/master/graph/badge.svg?token=HLMKRMA74L)](https://codecov.io/gh/omerfarukz/WebBen) [![CI & CD](https://github.com/omerfarukz/WebBen/actions/workflows/CI.yml/badge.svg)](https://github.com/omerfarukz/WebBen/actions/workflows/CI.yml)
[![CodeQL](https://github.com/omerfarukz/WebBen/actions/workflows/codeql.yml/badge.svg)](https://github.com/omerfarukz/WebBen/actions/workflows/codeql.yml) [![license](https://img.shields.io/github/license/omerfarukz/WebBen)](https://github.com/omerfarukz/WebBen/blob/master/LICENSE.txt)


Cross platform HTTP Server benchmark tool written in .NET 6.0. Declarative and easy to use. Execute test cases and
compare the
results. Parallelize the execution of test cases.

Runs on MacOS, Linux and Windows. For additional
information see [all supported platforms](https://github.com/dotnet/core/blob/main/release-notes/6.0/supported-os.md).

## Installation

```shell
dotnet tool install --global webben
```

## Usage samples

```shell
webben [command] [options]

Commands:
  config <fileInfo>
  uri <uri>
  analyse, analyze <uri>

Options:
  -v, --verbose                       Enable verbose output
  -e, --export-format <Default|Json>  Export format [default: Default]
  -?, -h, --help                      Show help and usage information

Example:
  webben config <filePath>

Output:
╭─────────────┬───────┬───┬───┬───┬────────┬──────────┬──────────╮
│Name         │Elapsed│NoR│Pll│Err│Avg(ms) │StdDev(ms)│Median(ms)│
├─────────────┼───────┼───┼───┼───┼────────┼──────────┼──────────┤
│get_simple_1 │2.13   │100│100│0  │2,083.83│11.66     │2,080.86  │
│get_simple_2 │2.03   │100│100│0  │2,017.24│7.21      │2,017.62  │
│post_simple_1│2.09   │100│500│0  │2,022.46│5.07      │2,020.69  │
╰─────────────┴───────┴───┴───┴───┴────────┴──────────┴──────────╯
```

```shell
webben uri <uri> [options]

Arguments:
  <uri>  The URI to use.

Options:
  -f, --fetch-content       Whether to fetch the content of the URI.
  -l, --name                Name or label
  -m, --http-method         The HTTP method to use.
  -n, --request-count       The number of requests to make.
  -p, --parallelism         The number of parallelism to use.
  -r, --allow-redirect      Whether to allow redirects.
  -t, --timeout-in-ms       The timeout in milliseconds.
  
Examples:
  webben uri http://localhost:3000
  webben uri http://localhost:3000 -n 10000
  webben uri http://localhost:3000 -p 100 -b 50 -t 5000 -m GET -f false -r false -n 10000

Output:
╭──────────────┬───────┬───┬───┬───┬───────┬──────────┬──────────╮
│Name          │Elapsed│NoR│Pll│Err│Avg(ms)│StdDev(ms)│Median(ms)│
├──────────────┼───────┼───┼───┼───┼───────┼──────────┼──────────┤
│initial_test_1│0.67   │10 │100│0  │456.69 │82.85     │423.60    │
╰──────────────┴───────┴───┴───┴───┴───────┴──────────┴──────────╯

```

```shell
Usage:
  webben analyze <uri> [options]
  webben analyse <uri> [options]

Arguments:
  <uri>  The URI to use.

Options:
  -f, --fetch-content                                      Whether to fetch the content of the URI.
  -r, --allow-redirect                                     Whether to allow redirects.
  -t, --timeout-in-ms <timeout-in-ms>                      The bounded capacity to use.
  -m, --max-trial-count <max-trial-count>                  Iteration count for calculation. See -c
  -c, --calculation-function <Average|Median|P70|P80|P90>  Function for RPS calculation

Examples:
  analyze "https://contoso.com/?q=test"
  analyze "https://contoso.com/?q=test" -c Median  
  analyze "https://contoso.com/?q=test" -m 5 -c P80

Output:
# Analyze result including max requests count per second
╭────────────────┬────────────┬───┬───┬───┬───────┬──────────┬──────────╮
│Name            │Elapsed(sec)│NoR│Pll│Err│Avg(ms)│StdDev(ms)│Median(ms)│
├────────────────┼────────────┼───┼───┼───┼───────┼──────────┼──────────┤
│2206260057093580│0.52        │1  │1  │0  │503.35 │0.00      │503.35    │
│2206260057093580│0.29        │2  │2  │0  │283.28 │1.34      │283.28    │
│2206260057093580│0.29        │4  │4  │0  │282.62 │11.45     │286.21    │
│2206260057093580│0.29        │8  │8  │0  │280.83 │8.95      │284.91    │
│2206260057093580│0.36        │16 │16 │0  │316.97 │19.17     │317.60    │
│2206260057093580│0.71        │32 │32 │0  │658.79 │30.66     │661.04    │
│2206260057093580│0.60        │64 │64 │0  │503.19 │59.68     │492.96    │
│2206260057093580│0.86        │128│128│0  │707.93 │87.33     │723.58    │
│2206260057093580│1.63        │256│256│0  │997.16 │161.11    │997.03    │
│2206260057093580│1.14        │129│129│0  │728.80 │72.92     │724.38    │
╰────────────────┴────────────┴───┴───┴───┴───────┴──────────┴──────────╯
╭──────╮
│MaxRPS│
├──────┤
│ 128  │
╰──────╯

```

### Analyzing session

![analyze2](https://raw.githubusercontent.com/omerfarukz/WebBen/master/Assets/analyze2.gif)

## Configuration Sample

```json
{
  "TestCaseConfigurations": [
    {
      "Name": "get_simple_1",
      "Uri": "http://localhost:3000/api/v1/posts/foo"
    },
    {
      "Name": "get_simple_2",
      "Uri": "http://localhost:3000/api/v1/posts/bar"
    },
    {
      "Name": "post_simple_1",
      "Uri": "http://localhost:3000/api/v1/posts",
      "HttpMethod": "POST",
      "NumberOfRequests": 500,
      "FetchContent": false,
      "Parallelism": 500,
      "BoundedCapacity": 500,
      "Headers": {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
      },
      "Cookies": {
        "X-Session-Id": "44c00ac0-eae8-11ec-8fea-0242ac120002"
      },
      "Body": {
        "Content": "{\"name\":\"test\"}",
        "ContentType": "application/json",
        "Encoding": "utf-8"
      }
    }
  ]
}
```

### Usage

```shell
./webben config samples/Multiple.json
```

### Output ( Default Formatter )

```shell
╭─────────────┬───────┬───┬───┬───┬────────┬────────┬────────╮
│Name         │Elapsed│NoR│Pll│Err│Avg     │P90     │Median  │
├─────────────┼───────┼───┼───┼───┼────────┼────────┼────────┤
│get_simple_1 │2.13   │100│100│0  │2,081.37│2,078.92│2,078.47│
│get_simple_2 │2.03   │100│100│0  │2,015.18│2,025.14│2,013.66│
│post_simple_1│2.08   │100│500│0  │2,019.53│2,023.18│2,019.60│
╰─────────────┴───────┴───┴───┴───┴────────┴────────┴────────╯
```

### Usage

```shell
./webben config samples/Multiple.json -e Json
```

### Output ( JSON Formatter )

```json
[{
  "Configuration": {
    "HttpMethod": "GET",
    "RequestCount": 100,
    "Parallelism": 100,
    "UseDefaultCredentials": false,
    "UseCookieContainer": false,
    "MaxBufferSize": 2147483647,
    "Name": "get_simple_1",
    "Uri": "http://localhost:3000/api/v1/posts/foo",
    "FetchContent": false,
    "AllowRedirect": false,
    "TimeoutInMs": 2147483647
  },
  "Timings": [
    "00:00:02.0780162",
    "...",
    "00:00:02.0726471"
  ],
  "Errors": [],
  "Elapsed": "00:00:02.1292561"
},
{
  "Configuration": {
    "HttpMethod": "GET",
    "RequestCount": 100,
    "Parallelism": 100,
    "UseDefaultCredentials": false,
    "UseCookieContainer": false,
    "MaxBufferSize": 2147483647,
    "Name": "get_simple_2",
    "Uri": "http://localhost:3000/api/v1/posts/bar",
    "FetchContent": false,
    "AllowRedirect": false,
    "TimeoutInMs": 2147483647
  },
  "Timings": [
    "00:00:02.0278468",
    "...",
    "00:00:02.0277827"
  ],
  "Errors": [],
  "Elapsed": "00:00:02.0749580"
}]
```

## Other features

Support for authentication, cookie containers, and default credentials. It is possible to use the same credential
configuration for multiple test cases. The credential configuration is specified by the `CredentialConfigurationKey`
property.

```json
{
  "CredentialConfigurations": [
    {
      "Key": "cred_1",
      "Provider": "NetworkCredentialProvider",
      "Data": {
        "username": "foo",
        "password": "bar"
      }
    }
  ]
}
```

Http request headers can be specified.

```json
{
  "TestCaseConfigurations": [
    {
      "Name": "post_simple_1",
      "Uri": "http://localhost:3000",
      "HttpMethod": "POST",
      "NumberOfRequests": 10000,
      "FetchContent": false,
      "Parallelism": 500,
      "BoundedCapacity": 500,
      "Headers": {
        "User-Agent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)"
      },
      "Body": {
        "Content": "{\"name\":\"test\"}",
        "ContentType": "application/json",
        "Encoding": "utf-8"
      }
    }
  ]
}
```
    
## Http request
Http requests created by WebBen are not actually sent to the server. Instead, they are stored in a queue. The queue is processed by multiple threads depending on the parallelism.

```http request
POST / HTTP/1.1
Host: localhost:3000
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) WebBen/1.2206
Content-Type: application/json; charset=utf-8
Content-Length: 15

{"name":"test"}
```

WebBen is just a simple tool to generate http requests. You can combine it with other tools to make more complex tasks. For example, you can use WebBen to generate requests and then use another tool to send them to the your backend.

### Example
```shell
webben uri https://url-to-benchmark.local/pages/1 -e json | \
curl -H "Content-Type: application/json" \
    -X POST \
    --data-binary @- \
    https://your-awesome-backend.local/api/v1/posts
    

POST / HTTP/1.1
Host: your-awesome-backend.local
User-Agent: curl/7.79.1
Accept: */*
Content-Type: application/json
Content-Length: 1042

{
  "Items": [
    {
      "Timings": [
        "00:00:00.5772976",
        "00:00:00.5528780",
        "00:00:00.5148466",
        "00:00:00.5118757",
        "00:00:00.5118554",
        "00:00:00.4524819",
        "00:00:00.5113236",
        "00:00:00.5117843",
        "00:00:00.5148185",
        "00:00:00.4524458"
      ],
      "Errors": [],
      "Elapsed": "00:00:00.5938226",
      "Configuration": {
        "HttpMethod": "GET",
        "RequestCount": 10,
        "Parallelism": 10,
        "UseDefaultCredentials": false,
        "UseCookieContainer": false,
        "MaxBufferSize": 2147483647,
        "Name": "iteration1",
        "Uri": "https://url-to-benchmark.local/pages/1",
        "FetchContent": false,
        "AllowRedirect": true,
        "TimeoutInMs": 2147483647
      },
      "Calculations": {
        "Average": "00:00:00.5111607",
        "StdDev": "00:00:00.0361018",
        "P90": "00:00:00.5772976",
        "P80": "00:00:00.5528780",
        "P70": "00:00:00.5148466",
        "Median": "00:00:00.5118655"
      }
    }
  ]
}
```

## :yum: How to contribute
Have an idea? Found a bug? Contributions are always welcome. 
- Open an issue or create a pull request.
- Share with your friends.
- Give a Star.

Thanks! :heart:
