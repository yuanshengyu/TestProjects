using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestReflection
{
    public class BaseEntity
    {
    }
    [MappingField("MonitorCollectData")]
    public class CollectDataEntity : BaseEntity
    {
        [MappingField("FID", false)]
        public int FID { get; set; }

        [MappingField("FDeviceID")]
        public string FDeviceID { get; set; }

        [MappingField("FBeginDate")]
        public DateTime? FBeginDate { get; set; }

        [MappingField("FZoneNo")]
        public int? FZoneNo { get; set; }
    }
}
