#JSON library for C#

##Usage

###Construction and serialization

```c#
JSON json_arr = new JSON() {
    0,
    "knock",
    "knock",
    "Neo"
};

json_arr.Add(true);

JSON json_obj = new JSON() {
    {"test", "foo"},
    {"bar", 24.5},
    {"boolean", false},
    {"something", null}
};

json_obj["new"] = json_arr;
json_obj["raw"] = 5;

string s1 = json_obj.ToString();
```

### Parsing

```
JSON json = new JSON("{\"key": \"value\"}");

string s = json["key"];
```

## Notes

This library hasn't been used in production yet and hasn't overgone many tests. However, it has been tested on some complicated production JSON structures and proved itself working.

For now, the library is quite tolerant and forgives some sorts of errors (especialy punctuational). Whether to make it more strict is still a matter of consideration.

Any fuzzy tests, bug reports and any other feedback would be very appreciated.

## TODO

* Polish API (assignment operators might not work as stated)
* Add tests
* Maybe, add more strict syntax checks
