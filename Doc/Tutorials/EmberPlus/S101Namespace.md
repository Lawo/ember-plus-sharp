The Lawo.EmberPlus.S101 Namespace    {#TheLawoEmberPlusS101Namespace}
=================================

[TOC]


Tutorial       {#TheLawoEmberPlusS101Namespace_Tutorial}
========

The library currently offers abstractions for S101 messages ([S101Message](\ref Lawo.EmberPlus.S101.S101Message)) plus
commands ([S101Command](\ref Lawo.EmberPlus.S101.S101Command),
[KeepAliveRequest](\ref Lawo.EmberPlus.S101.KeepAliveRequest),
[KeepAliveResponse](\ref Lawo.EmberPlus.S101.KeepAliveResponse), [EmberData](\ref Lawo.EmberPlus.S101.EmberData)) and
supports their encoding ([S101Writer](\ref Lawo.EmberPlus.S101.S101Writer)) and decoding
([S101Reader](\ref Lawo.EmberPlus.S101.S101Reader)).

The usage is demonstrated in the following test:

\snippet Lawo.EmberPlusTest/S101/S101WriterTest.cs Payload Test
