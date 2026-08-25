using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MESS.Application.Interfaces.Auth;
using MESS.Application.Interfaces.Org;
using MESS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MESS.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly MessDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOrgSyncService _orgSyncService;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        MessDbContext context,
        IPasswordHasher passwordHasher,
        IOrgSyncService orgSyncService,
        ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _orgSyncService = orgSyncService;
        _logger = logger;
    }

    public async Task<string> SeedAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer())
            {
                await _context.Database.MigrateAsync();
            }

            _logger.LogInformation("Bắt đầu kiểm tra và Seed Data hệ thống...");

            // 1. Seed Roles
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole == null)
            {
                adminRole = new Role { Id = Guid.NewGuid(), Name = "Admin", CreatedAt = DateTime.UtcNow };
                await _context.Roles.AddAsync(adminRole);
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (userRole == null)
            {
                userRole = new Role { Id = Guid.NewGuid(), Name = "User", CreatedAt = DateTime.UtcNow };
                await _context.Roles.AddAsync(userRole);
            }

            await _context.SaveChangesAsync();

            // 2. Seed Departments
            var deptBgd = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "BGD");
            if (deptBgd == null)
            {
                deptBgd = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Ban Giám Đốc",
                    Code = "BGD",
                    AutoCreateGroup = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Departments.AddAsync(deptBgd);
            }

            var deptKtsx = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "KTSX");
            if (deptKtsx == null)
            {
                deptKtsx = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Phòng Kỹ thuật Sản xuất",
                    Code = "KTSX",
                    AutoCreateGroup = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Departments.AddAsync(deptKtsx);
            }

            var deptKttc = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "KTTC");
            if (deptKttc == null)
            {
                deptKttc = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Phòng Kế toán & Tài chính",
                    Code = "KTTC",
                    AutoCreateGroup = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Departments.AddAsync(deptKttc);
            }

            var deptPxed = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "PXED");
            if (deptPxed == null)
            {
                deptPxed = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Phân xưởng Ép dập & Cơ khí",
                    Code = "PXED",
                    AutoCreateGroup = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Departments.AddAsync(deptPxed);
            }

            var deptQaqc = await _context.Departments.FirstOrDefaultAsync(d => d.Code == "QAQC");
            if (deptQaqc == null)
            {
                deptQaqc = new Department
                {
                    Id = Guid.NewGuid(),
                    Name = "Phòng Quản lý Chất lượng (QA/QC)",
                    Code = "QAQC",
                    AutoCreateGroup = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Departments.AddAsync(deptQaqc);
            }

            await _context.SaveChangesAsync();

            // 3. Seed Work Shifts
            var shift1 = await _context.WorkShifts.FirstOrDefaultAsync(s => s.Code == "SHIFT_01");
            if (shift1 == null)
            {
                shift1 = new WorkShift
                {
                    Id = Guid.NewGuid(),
                    Name = "Ca 1 - Ca Sáng",
                    Code = "SHIFT_01",
                    StartTime = new TimeSpan(6, 0, 0),
                    EndTime = new TimeSpan(14, 0, 0),
                    DepartmentId = deptPxed.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.WorkShifts.AddAsync(shift1);
            }

            var shift2 = await _context.WorkShifts.FirstOrDefaultAsync(s => s.Code == "SHIFT_02");
            if (shift2 == null)
            {
                shift2 = new WorkShift
                {
                    Id = Guid.NewGuid(),
                    Name = "Ca 2 - Ca Chiều",
                    Code = "SHIFT_02",
                    StartTime = new TimeSpan(14, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    DepartmentId = deptPxed.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.WorkShifts.AddAsync(shift2);
            }

            var shift3 = await _context.WorkShifts.FirstOrDefaultAsync(s => s.Code == "SHIFT_03");
            if (shift3 == null)
            {
                shift3 = new WorkShift
                {
                    Id = Guid.NewGuid(),
                    Name = "Ca 3 - Ca Đêm",
                    Code = "SHIFT_03",
                    StartTime = new TimeSpan(22, 0, 0),
                    EndTime = new TimeSpan(6, 0, 0),
                    DepartmentId = deptPxed.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.WorkShifts.AddAsync(shift3);
            }

            await _context.SaveChangesAsync();

            // 4. Seed Admin & Standard Users across departments
            var userSeeds = new List<(string username, string fullName, string password, Guid roleId, Guid deptId, Guid? shiftId, string position)>
            {
                // Ban Giám Đốc
                ("admin", "Quản trị viên Hệ thống", "Admin@123", adminRole.Id, deptBgd.Id, null, "Giám đốc Điều hành"),
                ("bgd1", "Lê Hoàng Long", "123456", userRole.Id, deptBgd.Id, null, "Phó Giám đốc Sản xuất"),

                // Phòng Kỹ thuật Sản xuất
                ("userA", "Nguyễn Văn A", "123456", userRole.Id, deptKtsx.Id, shift1.Id, "Trưởng phòng Kỹ thuật"),
                ("userB", "Trần Thị B", "123456", userRole.Id, deptKtsx.Id, shift1.Id, "Kỹ sư cơ khí"),
                ("ktsx1", "Phạm Quốc Huy", "123456", userRole.Id, deptKtsx.Id, shift2.Id, "Kỹ sư Tự động hóa"),

                // Phòng Kế toán & Tài chính
                ("ketoan1", "Đỗ Thu Hằng", "123456", userRole.Id, deptKttc.Id, null, "Kế toán Trưởng"),
                ("ketoan2", "Vũ Minh Châu", "123456", userRole.Id, deptKttc.Id, null, "Kế toán Chi phí & Vật tư"),

                // Phân xưởng Ép dập & Cơ khí
                ("epdap1", "Hoàng Văn Dũng", "123456", userRole.Id, deptPxed.Id, shift1.Id, "Quản đốc Phân xưởng"),
                ("epdap2", "Ngô Thành Nam", "123456", userRole.Id, deptPxed.Id, shift2.Id, "Tổ trưởng Ca Chiều"),
                ("epdap3", "Bùi Anh Tuấn", "123456", userRole.Id, deptPxed.Id, shift3.Id, "Kỹ thuật viên Vận hành Ca Đêm"),

                // Phòng Quản lý Chất lượng QA/QC
                ("qaqc1", "Trịnh Kim Ngân", "123456", userRole.Id, deptQaqc.Id, null, "Trưởng phòng QA/QC"),
                ("qaqc2", "Đặng Thái Sơn", "123456", userRole.Id, deptQaqc.Id, shift1.Id, "Chuyên viên Kiểm thử QC")
            };

            foreach (var seed in userSeeds)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == seed.username);
                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid(),
                        Username = seed.username,
                        FullName = seed.fullName,
                        PasswordHash = _passwordHasher.Hash(seed.password),
                        RoleId = seed.roleId,
                        DepartmentId = seed.deptId,
                        WorkShiftId = seed.shiftId,
                        PositionTitle = seed.position,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.Users.AddAsync(user);
                }
                else
                {
                    user.RoleId = seed.roleId;
                    user.DepartmentId = seed.deptId;
                    user.WorkShiftId = seed.shiftId;
                    user.PositionTitle = seed.position;
                    user.FullName = seed.fullName;
                    user.IsActive = true;
                }
            }

            await _context.SaveChangesAsync();

            // 5. Đồng bộ tất cả Nhóm chat theo Phòng ban và Ca trực tự động
            var syncResult = await _orgSyncService.SyncAllOrgGroupsAsync();

            _logger.LogInformation("Seed Data & Đồng bộ nhóm tổ chức hoàn tất! Kết quả: {Message}", syncResult.Message);
            return $"Seed data & Đồng bộ hoàn tất! Đã khởi tạo {userSeeds.Count} nhân viên cho 5 phòng ban và 3 ca trực. {syncResult.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Có lỗi xảy ra trong quá trình Seed Data!");
            throw;
        }
    }
}
