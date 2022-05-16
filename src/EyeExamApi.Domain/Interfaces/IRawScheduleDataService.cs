using EyeExamApi.Core.DTOs;

namespace EyeExamApi.Core.Interfaces
{
    public interface IRawScheduleDataService
    {
        IEnumerable<RawScheduleNoticeOfLease> GetRawScheduleNoticeOfLeases();
    }
}