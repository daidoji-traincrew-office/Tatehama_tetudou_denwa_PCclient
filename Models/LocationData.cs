using System.Collections.Generic;

namespace Tatehama_tetudou_denwa_PCclient.Models;

public static class LocationData
{
    public static List<CallListItem> GetLocations()
    {
        var locations = new List<CallListItem>
        {
            new CallListItem { DisplayName = "総合司令" },
            new CallListItem { DisplayName = "館浜駅乗務員詰所" },
            new CallListItem { DisplayName = "大道寺列車区" },
            new CallListItem { DisplayName = "赤山町駅乗務員詰所" },
            new CallListItem { DisplayName = "館浜" },
            new CallListItem { DisplayName = "駒野" },
            new CallListItem { DisplayName = "河原崎" },
            new CallListItem { DisplayName = "海岸公園" },
            new CallListItem { DisplayName = "虹ケ浜" },
            new CallListItem { DisplayName = "津崎" },
            new CallListItem { DisplayName = "浜園" },
            new CallListItem { DisplayName = "羽衣橋" },
            new CallListItem { DisplayName = "新井川" },
            new CallListItem { DisplayName = "新野崎" },
            new CallListItem { DisplayName = "江ノ原" },
            new CallListItem { DisplayName = "大道寺" },
            new CallListItem { DisplayName = "藤江" },
            new CallListItem { DisplayName = "水越" },
            new CallListItem { DisplayName = "高見沢" },
            new CallListItem { DisplayName = "日野森" },
            new CallListItem { DisplayName = "奥峰口" },
            new CallListItem { DisplayName = "西赤山" },
            new CallListItem { DisplayName = "赤山町" }
        };

        for (int i = 0; i < locations.Count; i++)
        {
            if (i < 4)
            {
                locations[i].PhoneNumber = (1001 + i).ToString();
            }
            else
            {
                locations[i].PhoneNumber = (2001 + (i - 4)).ToString();
            }
        }
        return locations;
    }
}
