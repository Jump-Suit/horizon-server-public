using RT.Common;

namespace RT.Models.Misc
{
    public interface IMediusAddToBuddyListConfirmationRequest
    {
        MediusBuddyAddType AddType { get; }
        int TargetAccountID { get; }
    }

    public interface IMediusAddToBuddyListConfirmationResponse
    {
        MediusBuddyAddType AddType { get; }
        int OriginatorAccountID { get; }
    }
}