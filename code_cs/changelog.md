# Changelog

All notable changes to this project will be documented in this file.

---

## 1.1.0
### Added
- Added Changelog
- Implementation of the STREAM command
- New sample application to outline streaming.
- Packet classes for reading data.
- Command classes for writing data.
- Listen thread for decoupling the original request-response method for commands.
- SeekableBufferedStream
  - A simple implementation of the IO Stream that allows seeking to previously read data.

### Changes
- Improved use of properties throughout classes.

### Fixed
- Fixed error where the BX parser would try to read the port status and frame number even if the handle status was not valid.

### Removed
- Connection protocol specific read/write functions

## 1.0.0

Original release showcasing basic functionality of using the Combined API in C#.

### Added
- Library wrapper of the CAPI.
- Application using the library for connecting, configuring and tracking with Aurora or Vega position sensors.

---

## Footnotes

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project loosely adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
