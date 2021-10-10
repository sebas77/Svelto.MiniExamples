# Changelog
All notable changes to this project will be documented in this file. Changes are listed in random order of importance.

## [3.2.0]

* ICompositionRoot OnContextDestroyed now receive a flag to know if the OnContextInitialized ever had the chance to be called
* Fix some naming case issues, not following the Svelto convention
* Improve FasterList interface
* Add Intersect/Exclude/Union methods to FasterDictionary to work with Sets
* Improve all the SveltoDictionary and derivates interfaces
* Changed (again and still not final) the logic behind the PlatformProfiler markers
* Refactor MemoryUtilities functionalities

## [3.1.3]

### Fixed

* fixed serious mistake in the RefWrapper equality logic that could affect certains kind of keys in the new FasterDictionary (most notable strings)

## [3.1.0]

### Changed

* UnityContext class won't call OnContextDestroyed on application quit
* RefWrapper, which is the way to have always struct keys in a fasterdictionary, now implicitly converts to the reference type it refers to
* Added a property to know if an IBuffer implementation buffer is valid
* FasterDictionary now wraps SveltoDictionary
* FasterDictionary doesn't accept reference types as key anymore, use RefWrapper to convert them
* renamed FasterDictionaryNode to SveltoDictionaryNode
* removed string based pools from the ObjectPool class
* added a very simple implementation of a sparseset

### Fixed

