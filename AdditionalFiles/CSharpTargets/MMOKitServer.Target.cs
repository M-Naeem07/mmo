// Copyright CodeSpartan, 2022. All rights reserved.

using UnrealBuildTool;
using System.Collections.Generic;

public class MMOKitServerTarget : TargetRules
{
	public MMOKitServerTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Server;
		DefaultBuildSettings = BuildSettingsVersion.V2;
		IncludeOrderVersion = EngineIncludeOrderVersion.Latest;
        bUseLoggingInShipping = true;
        ExtraModuleNames.AddRange( new string[] { "MMOKit" } );
	}
}
