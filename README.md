# WebBen[chmark]
[![dotnet build & pack](https://github.com/omerfarukz/WebBen/actions/workflows/CI-CD.yml/badge.svg)](https://github.com/omerfarukz/WebBen/actions/workflows/CI-CD.yml) [![nuget](https://img.shields.io/nuget/dt/webben)](https://www.nuget.org/packages/webben) [![CodeQL](https://github.com/omerfarukz/WebBen/actions/workflows/codeql.yml/badge.svg)](https://github.com/omerfarukz/WebBen/actions/workflows/codeql.yml) [![license](https://img.shields.io/github/license/omerfarukz/WebBen)](https://github.com/omerfarukz/WebBen/blob/master/LICENSE) 

Cross platform HTTP Server benchmark tool written in .NET 6.0. Declarative and easy to use. Execute test cases and
compare the
results. Parallelize the execution of test cases.

Runs on mac, linux and windows.

## Install

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
  -s, --buffer-size         The maximum size of the response content buffer.
  -t, --timeout-in-ms       The timeout in milliseconds.
  
Examples:
  webben uri http://localhost:3000
  webben uri http://localhost:3000 -n 10000
  webben uri http://localhost:3000 -p 100 -b 50 -t 5000 -m GET -f false -r false -n 10000

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
  Analyze result including max requests count per second

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

## Usage
```shell
./webben config samples/Multiple.json
```

## Output ( Default Exporter )

```shell
╭─────────────┬───────┬───┬───┬───┬────────┬────────┬────────╮
│Name         │Elapsed│NoR│Pll│Err│Avg     │P90     │Median  │
├─────────────┼───────┼───┼───┼───┼────────┼────────┼────────│
│get_simple_1 │2.13   │100│100│0  │2,085.22│2,082.91│2,082.21│
│get_simple_2 │2.03   │100│100│0  │2,016.29│2,025.80│2,015.81│
│post_simple_1│2.08   │100│500│0  │2,023.77│2,025.94│2,022.79│
╰─────────────┴───────┴───┴───┴───┴────────┴────────┴────────╯
```

## Usage
```shell
./webben config samples/Multiple.json -e Json
```

## Output ( JSON Exporter )
```json
[{
  "Configuration": {
    "HttpMethod": "GET",
    "RequestCount": 100,
    "Parallelism": 100,
    "UseDefaultCredentials": false,
    "UseCookieContainer": false,
    "MaxBufferSize": 2147483647,
    "CredentialConfigurationKey": null,
    "Headers": null,
    "Cookies": null,
    "Body": null,
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
    "CredentialConfigurationKey": null,
    "Headers": null,
    "Cookies": null,
    "Body": null,
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

### Http request

```http request
POST / HTTP/1.1
Host: localhost:3000
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64)
Content-Type: application/json; charset=utf-8
Content-Length: 15

{"name":"test"}
```

## :yum: How to contribute

Have an idea? Found a bug? Do not hasistate to contirubute. :rocket:

Thanks! :heart:

