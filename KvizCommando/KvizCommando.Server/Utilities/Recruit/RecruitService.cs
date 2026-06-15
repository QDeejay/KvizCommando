using KvizCommando.Server.Models;
using KvizCommando.Shared.Models;

namespace KvizCommando.Server.Utilities.Recruit
{
    public interface IRecruitService
    {
        RecruitSlot Generate(int count);
        (int[], int[], int[]) RecruitResolver(int member, int candidate);
    }

    public class RecruitService : IRecruitService
    {
        private readonly INameGenerator _names;
        private readonly IPicCodeGenerator _pics;

        public RecruitService(INameGenerator names, IPicCodeGenerator pics)
        {
            _names = names;
            _pics = pics;
        }

        public RecruitSlot Generate(int count)
        {
            return new RecruitSlot
            {
                Names = _names.GenerateNames(count),
                PictureCodes = _pics.GeneratePicCodes(count)
            };
        }
        public (int[], int[], int[]) RecruitResolver(int member, int candidate)
        {
            string m = RecruitData.OrientKeys[member-1];
            
            bool c = candidate==1 || candidate==3 ||candidate==5 || candidate==7 ? true : false;
            candidate--;
            bool[] mask = RecruitData.RecruitMask[candidate/2];
            int[] idx =  
            {
                int.Parse(m[0].ToString()),
                int.Parse(m[1].ToString()),
                int.Parse(m[2].ToString()),
                int.Parse(m[3].ToString()),
                int.Parse(m[4].ToString()),
                int.Parse(m[5].ToString()),
                int.Parse(m[6].ToString()),
                int.Parse(m[7].ToString()),
            };
           
            return (
                new int[]
                {
                    idx[0] + (mask[0] ? 8:0),
                    idx[1] + (mask[1] ? 8:0),
                    idx[0] + (mask[2] ? 8:0),
                    idx[1] + (mask[3] ? 8:0)},
                new int[] 
                {
                    (c ? idx[2] : idx[4]) + (mask[0] ? 8:0),
                    (c ? idx[3] : idx[5]) + (mask[1] ? 8:0),
                    (c ? idx[2] : idx[4]) + (mask[2] ? 8:0),
                    (c ? idx[3] : idx[5]) + (mask[3] ? 8:0)
                }, 
                new int[] 
                {
                    idx[6] + (mask[4] ? 8:0),
                    idx[6] + (mask[5] ? 8:0),
                    idx[7] + (mask[6] ? 8:0),
                    idx[7] + (mask[7] ? 8:0)
                });
        }
    }

}
