using Treevel.Common.Entities;
using Treevel.Common.Utils;

namespace Treevel.Common.Networks.Requests
{
    public class GetUserRecordRequest : GetServerRequestBase<UserRecord>
    {
        public GetUserRecordRequest()
        {
            key = Constants.PlayFabKeys.USER_RECORD;
        }
    }
}
