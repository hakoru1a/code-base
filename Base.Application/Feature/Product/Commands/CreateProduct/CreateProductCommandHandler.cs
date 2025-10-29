using Base.Infrastructure.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Application.Feature.Product.Commands.CreateProduct
{
    /*
     * 🧭 Ba cách phổ biến để xử lý nghiệp vụ trong Handler (tuỳ theo độ phức tạp):
     *
     * 1️ Xử lý trực tiếp (Simple Handler)
     *     - Dùng khi: logic đơn giản, chỉ thao tác với 1 aggregate, không có side-effect.
     *     - Bản chất: gọi trực tiếp repository để đọc/ghi dữ liệu.
     *     - Ví dụ business:
     *         + Cập nhật tên sản phẩm.
     *         + Thay đổi trạng thái đơn hàng (Pending → Approved).
     *
     * 2️ Domain Event (Event-driven)
     *     - Dùng khi: một hành động phát sinh nhiều phản ứng độc lập ở các nơi khác nhau.
     *     - Bản chất: publish–subscribe, mỗi handler xử lý riêng, không phụ thuộc thứ tự.
     *     - Ví dụ business:
     *         + Khi tạo Product → gửi email cho admin, ghi log, đồng bộ sang ElasticSearch.
     *         + Khi đơn hàng được giao → gửi SMS cho khách, cập nhật điểm thưởng, ghi audit log.
     *
     * 3️ UseCase / Orchestrator (Workflow-driven)
     *     - Dùng khi: nghiệp vụ phức tạp, cần xử lý nhiều bước có thứ tự, có điều kiện, có rollback.
     *     - Bản chất: điều phối (orchestrate) toàn bộ quy trình, thường gói thành 1 UseCase class.
     *     - Ví dụ business:
     *         + Khi tạo Order → kiểm tra khách hàng → kiểm tồn → tính thuế → tạo payment → commit.
     *         + Khi tạo phiếu nhập kho → validate nhà cung cấp → tính tổng chi phí → lưu → phát sự kiện.
     *
     * ⚙️ Quy tắc chọn:
     *     - Đơn giản, không side-effect  → dùng Handler gọi Repo trực tiếp.
     *     - Nhiều phản ứng độc lập        → dùng Domain Event.
     *     - Quy trình nhiều bước, phức tạp → dùng UseCase (Orchestrator).
     */

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, long>
    {
        private readonly IProductRepository _productRepository;

        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<long> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = Domain.Entities.Product.Create(
                request.Name,
                request.Description,
                request.Price,
                request.Stock,
                request.SKU
            );

            var productId = await _productRepository.CreateAsync(product);
            return productId;
        }
    }
}

