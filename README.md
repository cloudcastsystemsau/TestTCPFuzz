# TcpFuzzClient Test Documentation

[![.NET](https://github.com/cloudcastsystemsau/TestTCPFuzz/actions/workflows/dotnet.yml/badge.svg)](https://github.com/cloudcastsystemsau/TestTCPFuzz/actions/workflows/dotnet.yml)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blueviolet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/cloudcastsystemsau/TestTCPFuzz)](https://github.com/cloudcastsystemsau/TestTCPFuzz/blob/main/LICENSE)
![Platforms](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)
[![Last commit](https://img.shields.io/github/last-commit/cloudcastsystemsau/TestTCPFuzz)](https://github.com/cloudcastsystemsau/TestTCPFuzz/commits)
[![Issues](https://img.shields.io/github/issues/cloudcastsystemsau/TestTCPFuzz)](https://github.com/cloudcastsystemsau/TestTCPFuzz/issues)

This document provides comprehensive details of the fuzz tests implemented in the **TcpFuzzClient** tool.  
Each test case is designed to validate the robustness of a TCP server against malformed input, unusual connection patterns, and protocol confusion attacks.  
The following sections describe each test in detail, including the payloads used, their purpose, expected server behavior, and logging requirements.

---

## Test 1 — Send normal message
**Payload:** UTF-8 string `hello\n`  
**Purpose:** Baseline success path. Ensures the service correctly parses a well-formed, newline-terminated UTF-8 frame.  
**Expected Behavior:** Process message without error.  
**Failure:** Unexpected errors or crashes.  
**Logging:** Connection accepted, bytes received, parse success.

---

## Test 2 — Send invalid UTF-8
**Payload:** Bytes `C3 28` (invalid UTF-8 sequence).  
**Purpose:** Validate decoder error handling.  
**Expected Behavior:** Reject or sanitize input without crashing.  
**Failure:** Crashes, deadlocks, or resource leaks.  
**Logging:** Decoding error logged, frame handling outcome.

---

## Test 3 — Send partial message
**Payload:** `"partial-"` then `"message\n"` after delay.  
**Purpose:** Validate handling of fragmented TCP messages.  
**Expected Behavior:** Assemble into `"partial-message\n"` and process once.  
**Failure:** Parser fires early or times out.  
**Logging:** Frame assembly event, buffer length, outcome.

---

## Test 4 — Send large message
**Payload:** 8192 `A`s + newline (8193 bytes).  
**Purpose:** Validate maximum frame size handling.  
**Expected Behavior:** Accept or reject gracefully, without memory issues.  
**Failure:** OOM, crashes, or deadlocks.  
**Logging:** Frame length, decision (accepted/rejected), memory usage.

---

## Test 5 — Rapid open/close
**Payload:** N connections, each sends only `\n` then closes.  
**Purpose:** Stress connection lifecycle.  
**Expected Behavior:** Stable acceptance and cleanup, no leaks.  
**Failure:** Socket leaks, accept backlog exhaustion, degraded service.  
**Logging:** Connections per second, socket counts, cleanup metrics.

---

## Test 6 — Fuzz with garbage data
**Payload:** String with emojis, NULs, and newline: `💥🔥⚠️🐛\0\0\0\n`  
**Purpose:** Test Unicode + control char handling.  
**Expected Behavior:** Treat as valid frame with consistent sanitization.  
**Failure:** Truncation at NUL, mis-framing, crashes.  
**Logging:** Decoded code points, sanitization steps, frame outcome.

---

## Test 7 — Send fake TLS handshake
**Payload:** Malformed TLS ClientHello:  
```
16 03 01 00 2E 01 00 00 2A 03 03 53 43 4F 4D 0D 0A 00
```  
**Purpose:** Validate protocol confusion resistance.  
**Expected Behavior:** Reject quickly without CPU spikes or crashes.  
**Failure:** Server hangs, attempts bogus TLS negotiation, crashes.  
**Logging:** Protocol mismatch detection, reason for close, raw bytes logged.

---

## Pass/Fail Rubric

| Criteria   | Pass                                           | Fail                                               |
|------------|-----------------------------------------------|---------------------------------------------------|
| Safety     | No crashes, no leaks                          | Crashes, leaks, instability                       |
| Correctness| Valid frames parsed, invalid rejected cleanly | Mis-parsed frames, silent failures                |
| Resilience | Graceful handling of malformed input          | Starvation, blocking, deadlocks                   |
| Clarity    | Clear error logs                              | Ambiguous or missing error info                   |

---

## Sample Vendor Report Template

**Target:** `<IP:PORT>`  
**Date/Time:** `<YYYY-MM-DD hh:mm>`  
**Build:** `<server version/commit>`

| Test | Result | Key Observations | Metrics (before→after) | Logs/Artifacts |
|------|--------|------------------|------------------------|----------------|
| 1 Normal         | Pass/Fail | … | … | … |
| 2 Invalid UTF-8  | Pass/Fail | … | … | … |
| 3 Partial        | Pass/Fail | … | … | … |
| 4 Large          | Pass/Fail | … | … | … |
| 5 Rapid N=…      | Pass/Fail | … | … | … |
| 6 Garbage+NUL    | Pass/Fail | … | … | … |
| 7 Fake TLS       | Pass/Fail | … | … | … |

---

## Appendix — Exact Payloads

- **Test 1:** `68 65 6C 6C 6F 0A` (“hello\n”)  
- **Test 2:** `C3 28` (invalid UTF-8)  
- **Test 3:**  
  - Part 1: `"partial-"` → `70 61 72 74 69 61 6C 2D`  
  - Part 2: `"message\n"` → `6D 65 73 73 61 67 65 0A`  
- **Test 4:** `41` × 8192 then `0A`  
- **Test 5:** `0A` per connection (newline only)  
- **Test 6:** `"💥🔥⚠️🐛\0\0\0\n"` (valid UTF-8 + three NULs + newline)  
- **Test 7:** `16 03 01 00 2E 01 00 00 2A 03 03 53 43 4F 4D 0D 0A 00` (fake TLS ClientHello)

---

© Cloudcast Systems 2025 — For support contact: **support@cloudcastsystems.com**
