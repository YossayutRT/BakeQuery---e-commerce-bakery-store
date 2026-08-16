# BakeQuery

## Thai
BakeQuery เป็นระบบร้านเบเกอรี่ที่ออกแบบมาเพื่อช่วยให้ลูกค้าเลือกสินค้า สั่งซื้อออนไลน์ จัดการตะกร้า และติดตามสถานะคำสั่งซื้อได้อย่างง่ายดาย โดยนำเสนอประสบการณ์การใช้งานที่เข้าใจง่ายสำหรับผู้ใช้ทั่วไป และมีระบบหลังบ้านสำหรับผู้ดูแลร้าน เช่น การจัดการสินค้า โปรโมชั่น บัญชีผู้ใช้ และการตอบกลับออเดอร์

ระบบนี้พัฒนาด้วย ASP.NET Core MVC ร่วมกับฐานข้อมูล MySQL และมีการใช้ JWT สำหรับการเข้าสู่ระบบแบบปลอดภัย เพื่อให้ทั้งลูกค้าและผู้ดูแลมีความสะดวกและปลอดภัยในการใช้งาน

### จุดเด่นของระบบ
- ลูกค้าสามารถสมัครสมาชิกและเข้าสู่ระบบ
- เลือกดูสินค้าและหมวดหมู่ต่าง ๆ
- ใส่สินค้าในตะกร้าและปรับจำนวนสินค้า
- ใช้โค้ดโปรโมชั่นเพื่อรับส่วนลด
- ดูประวัติการสั่งซื้อและติดตามสถานะออเดอร์
- ผู้ดูแลสามารถจัดการสินค้า โปรโมชั่น และคำสั่งซื้อได้
- มีระบบแยกสิทธิ์ตามบทบาท เช่น ผู้ดูแล พนักงาน ลูกค้า

### เทคโนโลยีที่ใช้
- ASP.NET Core MVC
- C#
- MySQL
- Entity Framework Core
- JWT Authentication
- Session Management

> หมายเหตุ: แอปพลิเคชันมีการตั้งค่า JWT และ Connection String เรียบร้อยอยู่แล้ว หากใช้เครื่องอื่น ให้ปรับค่าใน `appsettings.json` ให้ถูกต้องก่อนรัน

### สิ่งที่โปรเจ็กต์นี้ทำได้
- ระบบร้านค้าทางออนไลน์สำหรับเบเกอรี่
- ระบบจัดการหลังร้านสำหรับ admin และ staff
- การจัดการโปรโมชั่น / ส่วนลด
- การจัดการคำสั่งซื้อและการชำระเงิน
- การยืนยันการชำระเงินแบบไฟล์ภาพ
- การแยกสิทธิ์ผู้ใช้งานตามบทบาท

---

## English
BakeQuery is a bakery store system designed to help customers browse products, place orders online, manage their cart, and track the status of their orders in a simple and user-friendly way. It also includes a back-office system for store management, allowing admins and staff to manage products, promotions, user accounts, and order responses efficiently.

This project is built with ASP.NET Core MVC and MySQL, and it uses JWT authentication to provide secure login and authorization for both customers and staff.

### Main Features
- Customer registration and login
- Product and category browsing
- Add products to cart and update quantities
- Apply promotional codes for discounts
- View order history and track order status
- Admin and staff can manage products, promotions, and orders
- Role-based access control for manager, admin, staff, and customer

### Technologies Used
- ASP.NET Core MVC
- C#
- MySQL
- Entity Framework Core
- JWT Authentication
- Session Management

### What This Project Can Do
- Online bakery storefront
- Back-office management for admin and staff
- Promotion and discount management
- Order management and payment handling
- Payment proof upload verification
- Role-based user authorization

---

## Summary / สรุป
BakeQuery เป็นโปรเจ็กต์ที่รวมฟังก์ชันร้านค้าออนไลน์และระบบจัดการร้านเบเกอรี่ไว้ในหนึ่งระบบ เพื่อให้ทั้งลูกค้าและผู้ดูแลสามารถใช้งานได้ง่ายและมีประสิทธิภาพ หากต้องการพัฒนาเพิ่มเติม สามารถต่อยอดได้หลายด้าน เช่น การชำระเงินออนไลน์ การจัดส่ง การรายงานยอดขาย หรือการเพิ่มระบบแอดมินที่ซับซ้อนขึ้น

BakeQuery is a project that combines an online store and bakery management system in one platform, making it easy and efficient for both customers and store administrators to use. It can be expanded further with online payment, delivery features, sales reports, and more advanced admin management.

## License
This project is intended for learning and academic use.

