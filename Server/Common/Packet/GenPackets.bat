START ../../PacketGenerator/bin/Debug/PacketGenerator.exe ../../PacketGenerator/PDL.xml
COPY /Y GenPackets.cs "../../DummyClient/Packet/GenPackets.cs"
COPY /Y GenPackets.cs "../../Server/Packet/GenPackets.cs"
COPY /Y ClientPacketManager.cs "../../DummyClient/Packet/ClientPacketManager.cs"
COPY /Y ServerPacketManager.cs "../../Server/Packet/ServerPacketManager.cs"