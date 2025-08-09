# Protocol

This document is a work in progress!

## Data types

| Name   | Size (bytes)        | Encodes                                             | Notes                        |
| ------ | ------------------- | --------------------------------------------------- | ---------------------------- |
| byte   | 1                   | An integer between 0 and 255                        |                              |
| uint16 | 2                   | An integer between 0 and 65,535                     |                              |
| uint32 | 4                   | An integer between 0 and 4,294,967,295              |                              |
| uint64 | 8                   | An integer between 0 and 18,446,744,073,709,551,615 |                              |
| float  | 4                   | 32-bit IEEE 754                                     |                              |
| bytes  | 4 + length of bytes | uint32                                              | Limit of 4,294,967,295 bytes |
| string | 4 + length of bytes | same as `bytes` but encoded/decoded as utf-8        |                              |

## Transports

### WebSocket

Connect to `wss://worker.retech.app`

### TLS

Not implemented yet (still working on the framing part)

### TCP

Not implemented yet (still working on the framing part)

## Packets

`CS`: Client to server
`SC`: Server to client

### CS 0x0000 Handshake

| Name        | Data type | value                      |
| ----------- | --------- | -------------------------- |
| Packet ID   | uint16    | `0x0000`                   |
| Game server | string    | `rust`                     |
| Version     | string    | `3.0.0`                    |
| Token       | string    | `JVQRtDzKePcaAX8AcwCMeyP6` |

### SC 0x0000 Handshake

| Name      | Data type | value    |
| --------- | --------- | -------- |
| Packet ID | uint16    | `0x0000` |
| Success   | byte      | `0x01`   |

### CS 0x0008 Chat

| Name      | Data type | value          |
| --------- | --------- | -------------- |
| Packet ID | uint16    | `0x0008`       |
| Time      | float     | `0`            |
| SteamID   | uint64    | `0`            |
| Channel   | string    | `global`       |
| Message   | string    | `Hello world!` |

### CS 0x0009 Voice

| Name       | Data type | value          |
| ---------- | --------- | -------------- |
| Packet ID  | uint16    | `0x0009`       |
| Time       | float     | `0`            |
| SteamID    | uint64    | `0`            |
| Voice data | bytes     | Raw voice data |
