import instance from "../axios";

export const sendOTP=async(email)=>{
    return await instance.post("/Auth/SendOTP",{email});
};

export const verifyOTP=async(email,otp)=>{
    return await instance.post("/Auth/VerifyOTP",{email,otp});
};