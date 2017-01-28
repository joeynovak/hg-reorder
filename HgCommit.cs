using System;

namespace mercurial_reorder
{
   internal class HgCommit : IComparable<HgCommit>
   {
      public string Branch = "";
      public int Revision;
      public string ChangesetId = "";
      public string ParentChangesetId = "";
      public DateTime Date = new DateTime();
      public string Message = "";

      public HgCommit(string data)
      {
         string[] pieces = data.Split(new string[] {"||||"}, StringSplitOptions.None);
         if(pieces.Length != 6)
            throw new InvalidOperationException("Does not have 6 fields: " + data);

         Revision = int.Parse(pieces[0]);
         Branch = pieces[1];
         ChangesetId = pieces[2];
         ParentChangesetId = pieces[3];
         Date = Epoch.FromUnixTime(Double.Parse(pieces[4]));
         Message = pieces[5];
      }

      public int CompareTo(HgCommit other)
      {
         return Revision.CompareTo(other.Revision);
      }
   }
}