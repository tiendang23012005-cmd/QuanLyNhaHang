export interface BanAnDto {
  maBan: number;
  soBan: string;
  trangThaiBan: string;
}

export interface DonHangNhanVienDto {
  maDonHang: number;
  maBan?: number;
  tenBan?: string;
  loaiDonHang?: string;
  ngayTao?: string;
  tongTien?: number;
  trangThaiDon?: string;
}

export interface NhanVienApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
}

export interface ChiTietDonHangDto {
  maChiTiet: number;
  maMonAn: number;
  tenMonAn: string;
  soLuong: number;
  giaLucDat: number;
  thanhTien: number;
  ghiChu?: string;
  trangThaiBep?: string;
}

export interface DonHangHienTaiDto {
  maDonHang: number;
  maBan?: number;
  soBan?: string;
  tongTien: number;
  trangThaiDon?: string;
  trangThaiThanhToan?: string;
  chiTietDonHangs: ChiTietDonHangDto[];
}

export interface MonAn {
  maMonAn: number;
  tenMonAn: string;
  gia: number;
}