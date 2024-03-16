// Copyright CodeSpartan, 2022. All rights reserved.

using UnrealBuildTool;
using System.Collections.Generic;

public class MMOKitClientTarget : TargetRules
{
	public MMOKitClientTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Client;
		DefaultBuildSettings = BuildSettingsVersion.V2;
		IncludeOrderVersion = EngineIncludeOrderVersion.Latest;
		//bUsesSteam = true;
		ExtraModuleNames.AddRange( new string[] { "MMOKit" } );
	}
}
