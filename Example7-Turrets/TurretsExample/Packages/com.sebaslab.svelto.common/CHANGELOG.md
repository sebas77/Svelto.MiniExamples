# Changelog
All notable changes to this project will be documented in this file. I created this file with Svelto.Common version 3.1.

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

