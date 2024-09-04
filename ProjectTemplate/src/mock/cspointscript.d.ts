// Instance: Object {
//   Msg: function Msg() { [native code] } 0 args
//   PublicMethod: function PublicMethod() { [native code] } 0 args
//   GetGameTime: function GetGameTime() { [native code] } 0 args
//   GetEntityOrigin: function GetEntityOrigin() { [native code] } 1 args
//   GameType: function GameType() { [native code] } 0 args
//   GameMode: function GameMode() { [native code] } 0 args
//   IsWarmupPeriod: function IsWarmupPeriod() { [native code] } 0 args
//   GetPlayerPawn: function GetPlayerPawn() { [native code] } 1 args
//   EntFireAtName: function EntFireAtName() { [native code] } 4 args
//   DebugScreenText: function DebugScreenText() { [native code] } 7 args
//   EntFireBroadcast: function EntFireBroadcast() { [native code] } 4 args
//   InitialActivate: function InitialActivate() { [native code] } 1 args
// }

interface Instance_t {
  Msg: (_msg: string) => void;
  PublicMethod: (_name: string, _func: () => void) => void;
}

export declare const Instance: Instance_t;
