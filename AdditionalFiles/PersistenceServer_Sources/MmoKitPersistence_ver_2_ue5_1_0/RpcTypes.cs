﻿namespace PersistenceServer
{
    public enum RpcType { 
        RpcUndef, // 0
        RpcConnected, // 1
        RpcDisconnected, // 2
        RpcCreateAccountPassword, //3
        RpcCreateAccountSteam, // 4
        RpcLoginPassword, // 5
        RpcLoginSteam, // 6
        RpcMessageChannel, // 7
        RpcMessagePlayer, // 8
        RpcMessageParty, // 9
        RpcMessageGuild, // 10
        RpcGroupInvite, // 11
        RpcGroupLeave, // 12
        RpcGroupKick, // 13
        RpcAcceptInvite, // 14
        RpcDeclineInvite, // 15
        RpcGuildCreate, // 16
        RpcGuildInvite, // 17
        RpcGuildDisband, // 18
        RpcGuildLeave, // 19
        RpcCreateCharacter, // 20
        RpcGetCharacters, // 21
        RpcGetIpAndPort, // 22
        RpcGetCharacter, // 23
        RpcLoginServer, // 24
        RpcLoginClientWithCookie, // 25
        RpcSaveCharacter, // 26
        RpcNoSuchPlayer, // 27
        RpcKeepAliveProbe, // 28
    }
}
