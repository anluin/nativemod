#![no_std]
#![allow(non_snake_case)]

mod vintagestory;

#[panic_handler]
fn panic_handler(_info: &core::panic::PanicInfo) -> ! {
    loop {}
}

#[no_mangle]
extern "C" fn start() {
    vintagestory::logger::debug("Hello, world!");
}

