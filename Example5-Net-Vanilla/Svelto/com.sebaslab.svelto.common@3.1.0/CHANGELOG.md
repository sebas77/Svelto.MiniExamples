# Changelog
All notable changes to this project will be documented in this file. I created this file with Svelto.Common version 3.1.

## [0.3.1]

### Changed

* UnityContext class won't call OnContextDestroyed on application quit
* RefWrapper, which is the way to have always struct keys in a fasterdictionary, now implicitly concert to the reference type it refers to
* Added property to know if an IBuffer implementation buffer is valid
* FasterDictionary now wraps SveltoDictionary
* FasterDictionary doesn't accept reference types as key anymore, use RefWrapper to convert them
* renamed FasterDictionaryNode to SveltoDictionaryNode
* removed string based pools fro the ObjectPool
* added a very simple implementation of a sparseset
* 

### Fixed

