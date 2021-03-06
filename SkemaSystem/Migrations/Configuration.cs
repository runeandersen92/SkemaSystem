namespace SkemaSystem.Migrations
{
    using SkemaSystem.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Security;

    internal sealed class Configuration : DbMigrationsConfiguration<SkemaSystem.Models.SkeamSystemDb>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(SkemaSystem.Models.SkeamSystemDb context)
        {
            Teacher t1 = new Teacher { Id = 1, Name = "Hanne Sommer", Username = "eaasommer", Password = "fisk123", Role = Models.Enum.UserRoles.Teacher };
            Teacher t2 = new Teacher { Id = 2, Name = "Torben Kr�jmand", Username = "eaatk", Password = "fisk123", Role = Models.Enum.UserRoles.Teacher };
            Teacher t3 = new Teacher { Id = 3, Name = "Erik Jacobsen", Username = "eaaej", Password = "fisk123", Role = Models.Enum.UserRoles.Admin };
            Teacher t4 = new Teacher { Id = 4, Name = "J�rn Hujak", Username = "eaajh", Password = "fisk123", Role = Models.Enum.UserRoles.Teacher };
            Teacher t5 = new Teacher { Id = 5, Name = "Karsten ITO", Username = "eaakarsten", Password = "fisk123", Role = Models.Enum.UserRoles.Teacher };

            ClassModel c1 = new ClassModel { Id = 1, ClassName = "12t" };
            ClassModel c2 = new ClassModel { Id = 2, ClassName = "12s" };

            Subject su1 = new Subject { Id = 1, Name = "SK" };
            Subject su2 = new Subject { Id = 2, Name = "SD" };
            Subject su3 = new Subject { Id = 3, Name = "ITO" };

            SemesterSubjectBlock ssb1 = new SemesterSubjectBlock { Id = 1, Subject = su1, BlocksCount = 40 };
            SemesterSubjectBlock ssb2 = new SemesterSubjectBlock { Id = 2, Subject = su2, BlocksCount = 20 };
            SemesterSubjectBlock ssb3 = new SemesterSubjectBlock { Id = 3, Subject = su3, BlocksCount = 20 };
            SemesterSubjectBlock ssb4 = new SemesterSubjectBlock { Id = 4, Subject = su1, BlocksCount = 10 };
            SemesterSubjectBlock ssb5 = new SemesterSubjectBlock { Id = 5, Subject = su1, BlocksCount = 40 };
            SemesterSubjectBlock ssb6 = new SemesterSubjectBlock { Id = 6, Subject = su1, BlocksCount = 50 };

            List<SemesterSubjectBlock> ssdbList1 = new List<SemesterSubjectBlock>();
            ssdbList1.Add(ssb1); ssdbList1.Add(ssb2); ssdbList1.Add(ssb3);

            List<SemesterSubjectBlock> ssdbList2 = new List<SemesterSubjectBlock>();
            ssdbList2.Add(ssb4);

            List<SemesterSubjectBlock> ssdbList3 = new List<SemesterSubjectBlock>();
            ssdbList3.Add(ssb5);

            List<SemesterSubjectBlock> ssdbList4 = new List<SemesterSubjectBlock>();
            ssdbList4.Add(ssb6);


            Semester s1 = new Semester { Id = 1, Number = 1, Blocks = ssdbList1 };
            Semester s2 = new Semester { Id = 2, Number = 2, Blocks = ssdbList2 };
            Semester s3 = new Semester { Id = 3, Number = 3, Blocks = ssdbList3 };
            Semester s4 = new Semester { Id = 4, Number = 4, Blocks = ssdbList4 };

            Education e1 = new Education { Id = 1, Name = "DMU", NumberOfSemesters = 4 };
            Education e2 = new Education { Id = 2, Name = "MDU", NumberOfSemesters = 6 };

            Scheme sch1 = new Scheme { ClassModel = c1, Semester = s1, Id = 1 };
            Scheme sch2 = new Scheme { ClassModel = c1, Semester = s2, Id = 2 };
            Scheme sch3 = new Scheme { ClassModel = c2, Semester = s1, Id = 3 };

            Room r1 = new Room { Id = 1, RoomName = "SH-A1.13" };
            Room r2 = new Room { Id = 2, RoomName = "SH-A1.8" };
            Room r3 = new Room { Id = 3, RoomName = "SH-A1.4" };

            List<Teacher> tList = new List<Teacher>();
            tList.Add(t1);
            tList.Add(t2);
            tList.Add(t3);
            tList.Add(t4);
            tList.Add(t5);

            e1.Teachers = tList;

            List<Semester> sList = new List<Semester>();
            sList.Add(s1);
            sList.Add(s2);
            sList.Add(s3);
            sList.Add(s4);

            e1.Semesters = sList;

            c1.Education = e1;
            c2.Education = e1;


            context.Educations.AddOrUpdate(
                e => e.Id,
                e1, e2

            );

            context.Teachers.AddOrUpdate(
                t => t.Id,
                t1, t2, t3, t4, t5
            );


            context.Classes.AddOrUpdate(
                c => c.Id,
                c1, c2
                );

            context.Subjects.AddOrUpdate(
                s => s.Id,
                su1, su2, su3);

            context.Semesters.AddOrUpdate(
                s => s.Id,
                s1, s2, s3, s4);

            context.Rooms.AddOrUpdate(
                r => r.Id,
                r1, r2, r3
            );
        }
    }
}
