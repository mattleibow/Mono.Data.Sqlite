//
// Type forwarding to avoid namespace clashes
//

// The DBNull is a fake that is used in PCL. 
// It doesn't exist in the Metro (WinRT) framework
[assembly: System.Runtime.CompilerServices.TypeForwardedTo(typeof(System.DBNull))]