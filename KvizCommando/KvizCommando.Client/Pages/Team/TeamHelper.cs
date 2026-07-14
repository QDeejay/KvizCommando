using KvizCommando.Client.Helpers;
using KvizCommando.Client.Models.ViewModels.Ui;
using KvizCommando.Shared.Models;
using System.Globalization;

namespace KvizCommando.Client.Pages.Team
{
    internal sealed class TeamHelper
    {
       
        internal static string FormatOneDecimal(double value, bool percent)
        {
            if (percent)
            {
                return value.ToString() + "%";
            }
            else
            {
                double truncated = Math.Truncate(value * 10) / 10.0;
                return truncated.ToString("0.0", CultureInfo.InvariantCulture) + "s";
            }

        }
        internal static (int, int, int[]) RecruitResolver(int member, int candidate)
        {
            string m = RecruitData.OrientKeys[member - 1];

            bool c = candidate == 1 || candidate == 3 || candidate == 5 || candidate == 7 ? true : false;
            candidate--;
            bool[] mask = RecruitData.RecruitMask[candidate / 2];
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

            return (idx[0], c ? idx[2] : idx[4],
                new int[]
                {
                    idx[0] + (mask[0] ? 8:0),
                    (c ? idx[2] : idx[4]) + (mask[0] ? 8:0),
                    idx[6] + (mask[4] ? 8:0),
                    idx[1] + (mask[1] ? 8:0),
                    (c ? idx[3] : idx[5]) + (mask[1] ? 8:0),
                    idx[6] + (mask[5] ? 8:0)
                });
        }

        internal static List<SubHeaderVm> SubHeaderResolver(bool[] masks, string cult)
        {
            var list = new List<SubHeaderVm>();
            int index = 0;
            foreach (var mask in masks)
            {
                index++;
                list.Add(new SubHeaderVm
                {
                    Text = OrientationLocalizer.GetOrientation(index, cult),
                    Enable = mask,
                    Visible = mask,
                    ClickId = index,
                });
            }
            return list;
        }
    }
}

