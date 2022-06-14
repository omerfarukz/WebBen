# WebBen

Yet another http benchmark tool written in .NET 6.0. Declarative and easy to use. Execute test cases and compare the
results. Parallelize the execution of test cases.

## Usage samples

```shell
Usage:
  webben [command] [options]

Commands:
  config <fileInfo>
  uri <uri>

Example:
  webben config <filePath>

Usage:
  webben uri <uri> [options]

Arguments:
  <uri>  The URI to use.

Options:
  -p, --parallelism         The number of parallelism to use.
  -b, --bounded-capacity    The bounded capacity to use.
  -t, --timeout-in-ms       The timeout in milliseconds.
  -m, --http-method         The HTTP method to use.
  -f, --fetch-content       Whether to fetch the content of the URI.
  -r, --allow-redirect      Whether to allow redirects.
  -n, --request-count       The number of requests to make.
  -s, --buffer-size         The maximum size of the response content buffer.
  -?, -h, --help            Show help and usage information

Examples:
  webben uri http://localhost:3000
  webben uri http://localhost:3000 -n 10000
  webben uri http://localhost:3000 -p 100 -b 50 -t 5000 -m GET -f false -r false -n 10000
```

## Configuration Sample

```json
{
  "TestCaseConfigurations": [
    {
      "Name": "search_term_page_2_ntlm",
      "Uri": "https://internal.contoso.com/Ajax.ashx?search=term&page=2",
      "NumberOfRequests": 50,
      "FetchContent": false,
      "Parallelism": 50,
      "BoundedCapacity": 50,
      "CredentialConfigurationKey": "cred_1"
    },
    {
      "Name": "another_uri_anonym",
      "Uri": "https://public.contoso.com/",
      "HttpMethod": "GET",
      "NumberOfRequests": 10000,
      "FetchContent": false,
      "Parallelism": 500,
      "BoundedCapacity": 500,
      "UseDefaultCredentials": false,
      "UseCookieContainer": false
    }
  ],
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

## Output

```shell
 | Name                    | NoR  | Pll | BC  | Err | Avg    | Min   | Max      | P90    | P80    | Median | 
 |---------------------------------------------------------------------------------------------------------| 
 | search_term_page_2_ntlm | 1000 | 500 | 250 | 0   | 345.50 | 25.36 | 1,240.98 | 924.09 | 748.85 | 248.23 | 
 | another_uri_anonym      | 1000 | 100 | 100 | 0   | 125.02 | 74.94 | 240.27   | 193.38 | 168.92 | 101.66 |
```

## Other features

Support for NTLM authentication, cookie containers, and default credentials. It is possible to use the same credential
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
