export const userHelper = {
    getUserIdFromToken: () => {
        const token = localStorage.getItem('token');
        if (!token) return null;
        const tokenParts = JSON.parse(atob(token.split('.')[1]));
        return tokenParts.userid;
    },};
