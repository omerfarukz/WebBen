# WebBen[chmark]
[![dotnet build & pack](https://github.com/omerfarukz/WebBen/actions/workflows/CI-CD.yml/badge.svg)](https://github.com/omerfarukz/WebBen/actions/workflows/CI-CD.yml) [![nuget](https://img.shields.io/nuget/dt/webben)](https://www.nuget.org/packages/webben) [![CodeQL](https://github.com/omerfarukz/WebBen/actions/workflows/codeql.yml/badge.svg)](https://github.com/omerfarukz/WebBen/actions/workflows/codeql.yml) [![license](https://img.shields.io/github/license/omerfarukz/WebBen)](https://github.com/omerfarukz/WebBen/blob/master/LICENSE) 

Cross platform HTTP Server benchmark tool written in .NET 6.0. Declarative and easy to use. Execute test cases and
compare the
results. Parallelize the execution of test cases.

Runs on MacOS, Linux and Windows. For additional information: [all supported platforms](https://github.com/dotnet/core/blob/main/release-notes/6.0/supported-os.md).

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

### Analyze output sample
╭────────────────┬───────┬───┬───┬───┬──────┬──────┬────────┬──────╮
│Name            │Elapsed│NoR│Pll│Err│Avg   │StdDev│P90     │Median│
├────────────────┼───────┼───┼───┼───┼──────┼──────┼────────┼──────┤
│2206251705577109│0.50   │1  │1  │0  │486.76│0.00  │486.76  │486.76│
│2206251705577109│0.19   │1  │1  │0  │184.69│0.00  │184.69  │184.69│
│2206251705577109│0.18   │1  │1  │0  │183.66│0.00  │183.66  │183.66│
│2206251705577109│0.20   │2  │2  │0  │187.48│8.98  │196.46  │187.48│
│2206251705577109│0.19   │2  │2  │0  │188.57│2.82  │191.39  │188.57│
│2206251705577109│0.17   │2  │2  │0  │166.83│1.87  │168.70  │166.83│
│2206251705577109│0.19   │4  │4  │0  │180.34│10.00 │190.32  │180.82│
│2206251705577109│0.18   │4  │4  │0  │173.27│6.08  │180.06  │173.70│
│...             │....   │.. │.. │.. │..    │..    │..      │..    │
│2206251705577143│0.84   │99 │99 │0  │468.69│70.77 │539.09  │459.48│
│2206251705577144│0.74   │99 │99 │0  │474.65│91.36 │594.41  │454.96│
│2206251705577144│0.64   │100│100│0  │462.33│84.57 │600.63  │440.70│
│2206251705577145│0.79   │100│100│0  │476.01│73.05 │547.46  │474.23│
│2206251705577145│1.32   │100│100│0  │735.83│396.08│1,271.92│446.48│
│2206251705577146│0.78   │101│101│0  │463.78│73.57 │564.22  │448.14│
│2206251705577146│0.81   │101│101│0  │555.94│162.18│798.91  │467.72│
│2206251705577147│1.70   │101│101│0  │682.53│369.36│1,353.52│459.46│
╰────────────────┴───────┴───┴───┴───┴──────┴──────┴────────┴──────╯
Best RPS is 100
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
├─────────────┼───────┼───┼───┼───┼────────┼────────┼────────┤
│get_simple_1 │2.13   │100│100│0  │2,081.37│2,078.92│2,078.47│
│get_simple_2 │2.03   │100│100│0  │2,015.18│2,025.14│2,013.66│
│post_simple_1│2.08   │100│500│0  │2,019.53│2,023.18│2,019.60│
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

