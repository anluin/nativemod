static mut LOGGER_DEBUG_FN: Option<extern fn(*const u8)> = None;

#[no_mangle]
extern "C" fn set_logger_debug_fn(callback: extern fn(*const u8)) {
    unsafe { LOGGER_DEBUG_FN = Some(callback); }
}

pub fn debug(message: &str) {
    unsafe { LOGGER_DEBUG_FN.unwrap()(message.as_ptr()); }
}
