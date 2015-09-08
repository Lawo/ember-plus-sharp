The Lawo.EmberPlus Library    {#TheLawoEmberPlusLibrary}
==========================

[TOC]


Prerequisites       {#TheLawoEmberPlusLibrary_Prerequisites}
=============

This library implements the protocol described in the
[Ember+ Specification](http://ember-plus.googlecode.com/svn/trunk/documentation/Ember+%20Documentation.pdf). If you are
unfamiliar with <b>Ember+</b>, it is recommended to read the *Introduction* (currently pages 7 & 8).


Overview       {#TheLawoEmberPlusLibrary_Overview}
========

First and foremost, this library provides the means to quickly implement an <b>Ember+</b> consumer in code, which can
then be used in an application that needs to communicate with an existing <b>Ember+</b> provider. Most of the types
supporting this use case are demonstrated on the \ref TheLawoEmberPlusModelNamespace page.

As a byproduct, this library also offers support for lower level <b>Ember+</b> communication, as described on the
\ref TheLawoEmberPlusEmberNamespace and \ref TheLawoEmberPlusS101Namespace pages.

All classes in the library are transport-agnostic. <b>Ember+</b> protocol messages can be sent over any bidirectional
connection-oriented transport capable of sending and receiving bytes, like e.g. TCP/IP, named pipes, RS232, and so on.


Future Directions        {#TheLawoEmberPlusLibrary_FutureDirections}
=================

The following <b>Ember+</b> features are not currently supported but will be implemented when the need arises:

1. [Matrices](https://redmine.lawo.de/redmine/issues/1309)
2. [Streams](https://redmine.lawo.de/redmine/issues/1314)
3. [S101 out-of-frame bytes](https://redmine.lawo.de/redmine/issues/1315)
4. S101 Versioning
   - Messages with a Message Type not equal to 0x0E
   - Commands with a Version not equal to 0x01


Known Issues/Limitations        {#TheLawoEmberPlusLibrary_KnownIssues}
========================

Indefinite Length Form for Outgoing Messages        {#TheLawoEmberPlusLibrary_KnownIssues_IndefiniteLengthForm}
--------------------------------------------

Whenever permitted by the specification, the library uses the indefinite length form for payloads of outgoing messages
with [EmberData](Lawo.EmberPlus.S101.EmberData) commands (both forms are supported for incoming messages). This
simplifies the implementation and reduces the necessary memory and CPU resources to assemble a message at the expense of
message size.

Informal measurements have shown the following overheads:
- Small messages with nested containers and little other data:
  - 100% for a Glow message with an empty RootElementCollection (worst case)
  - ~50% for typical messages
- <20% for larger messages with more data. For example, the overhead for the following message is 10.39%:
  ~~~xml
  <Root type="RootElementCollection">
    <RootElement type="Node">
      <number type="Integer">7</number>
      <children type="ElementCollection">
        <Element type="Node">
          <number type="Integer">5</number>
          <children type="ElementCollection">
            <Element type="Node">
              <number type="Integer">0</number>
              <children type="ElementCollection">
                <Element type="Node">
                  <number type="Integer">0</number>
                  <contents type="Set">
                    <identifier type="UTF8String">Properties</identifier>
                    <description type="UTF8String">Properties</description>
                    <isOnline type="Boolean">true</isOnline>
                  </contents>
                </Element>
                <Element type="Node">
                  <number type="Integer">1</number>
                  <contents type="Set">
                    <identifier type="UTF8String">TestStaticItems</identifier>
                    <description type="UTF8String">TestStaticItems</description>
                    <isOnline type="Boolean">true</isOnline>
                  </contents>
                </Element>
                <Element type="Node">
                  <number type="Integer">2</number>
                  <contents type="Set">
                    <identifier type="UTF8String">TestDynamicItems</identifier>
                    <description type="UTF8String">TestDynamicItems</description>
                    <isOnline type="Boolean">true</isOnline>
                  </contents>
                </Element>
              </children>
            </Element>
          </children>
        </Element>
      </children>
    </RootElement>
  </Root>
  ~~~

Bandwidth-wise it seems the indefinite length form approach will have a barely noticeable impact because the library
allows for value changes to multiple parameters to be sent in a single message:
- A change that involves many parameters will thus be sent in a large message where the overhead is typically below 20%.
- Changes involving few parameters will be relatively rare with small message sizes, which makes the large percental
  overheads insignificant.


Doxygen        {#TheLawoEmberPlusLibrary_KnownIssues_Doxygen}
-------

- Generic classes are not linked in the documentation due to a **Doxygen** limitation.
- References to .NET Framework classes are not links due to a **Doxygen** limitation.
- Copy-pasting the contents of a code block in **Internet Explorer** leads to badly formatted code.
