<?php

return [

    'paths' => ['api/*', 'oauth/*', 'sanctum/csrf-cookie'],

    'allowed_methods' => ['*'],

    'allowed_origins' => [
        'https://cliniclaravel.lmp.id.vn',
        // nếu có base domain
        'http://clinic-management-project-mu.vercel.app',
        // môi trường dev (nếu build từ local)
        'http://localhost:5173',
    ],

    'allowed_origins_patterns' => [],

    'allowed_headers' => ['*'],

    'supports_credentials' => true,

    'exposed_headers' => [],

    'max_age' => 0,
];
