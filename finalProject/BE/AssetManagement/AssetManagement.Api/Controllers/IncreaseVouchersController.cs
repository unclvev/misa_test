using AssetManagement.Application.Dtos;
using AssetManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncreaseVouchersController : ControllerBase
{
    private readonly IIncreaseVoucherService _voucherService;

    public IncreaseVouchersController(IIncreaseVoucherService voucherService)
    {
        _voucherService = voucherService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResultDto<IncreaseVoucherListDto>>> GetAllVouchers(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _voucherService.GetAllVouchersPaged(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy danh sách chứng từ ghi tăng", error = ex.Message });
        }
    }

    [HttpGet("filter")]
    public async Task<ActionResult<PagedResultDto<IncreaseVoucherListDto>>> FilterVouchers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchText = null,
        [FromQuery] DateOnly? fromDate = null,
        [FromQuery] DateOnly? toDate = null,
        [FromQuery] decimal? totalOriginalPrice = null)
    {
        try
        {
            var result = await _voucherService.FilterVouchers(pageNumber, pageSize, searchText, fromDate, toDate, totalOriginalPrice);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lọc chứng từ ghi tăng", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IncreaseVoucherDto>> GetVoucherById(ulong id)
    {
        try
        {
            var voucher = await _voucherService.GetVoucherById(id);
            if (voucher == null)
            {
                return NotFound(new { message = $"Không tìm thấy chứng từ với ID {id}" });
            }
            return Ok(voucher);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy thông tin chứng từ", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<IncreaseVoucherDto>> CreateVoucher([FromBody] RequestIncreaseVoucherDto createVoucherDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdVoucher = await _voucherService.CreateVoucher(createVoucherDto);
            return Ok(createdVoucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo chứng từ", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<IncreaseVoucherDto>> UpdateVoucher(ulong id, [FromBody] RequestIncreaseVoucherDto updateVoucherDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedVoucher = await _voucherService.UpdateVoucher(id, updateVoucherDto);
            return Ok(updatedVoucher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi cập nhật chứng từ", error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVoucher(ulong id)
    {
        try
        {
            var result = await _voucherService.DeleteVoucher(id);
            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy chứng từ với ID {id}" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa chứng từ", error = ex.Message });
        }
    }

    [HttpDelete("{voucherId}/details/{detailId}")]
    public async Task<IActionResult> DeleteVoucherDetail(ulong voucherId, ulong detailId)
    {
        try
        {
            var result = await _voucherService.DeleteVoucherDetail(detailId);
            if (!result)
            {
                return NotFound(new { message = $"Không tìm thấy chi tiết chứng từ với ID {detailId}" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa chi tiết chứng từ", error = ex.Message });
        }
    }

    [HttpDelete("bulk-delete")]
    public async Task<IActionResult> BulkDelete([FromBody] List<ulong> ids)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest(new { message = "Danh sách ids rỗng." });

        try
        {
            var deletedCount = await _voucherService.DeleteManyAsync(ids);
            return Ok(new { deleted = deletedCount });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi xóa các chứng từ", error = ex.Message });
        }
    }

    [HttpGet("check-voucher-no/{voucherNo}")]
    public async Task<ActionResult<bool>> CheckVoucherNo(string voucherNo)
    {
        var exists = await _voucherService.IsVoucherNoExists(voucherNo);
        return Ok(new { exists });
    }

    [HttpGet("next-voucher-no")]
    public async Task<ActionResult<string>> GetNextVoucherNo()
    {
        try
        {
            var nextNo = await _voucherService.GetNextVoucherNo();
            return Ok(new { voucherNo = nextNo });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi lấy số chứng từ tiếp theo", error = ex.Message });
        }
    }
}

