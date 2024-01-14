resource "hcloud_network" "network" {
   name = "main_network"
   ip_range = "10.0.0.0/16"
}

resource "hcloud_network_subnet" "subnet" {
    type = "cloud"
    network_id = hcloud_network.network.id
    network_zone = "eu-central"
    ip_range = "10.0.1.0/24"
}

module "beckend_server" {
    source = "./modules/beckend_server"

    name = "backend"
    location = "Helsinki"
    server_type = "cx21"
    image = "ubuntu-22.04"
    ssh_key = [data.hcloud_ssh_key.ssh_key.name]
    network_id = hcloud_network.network.id
}

module "database_server" {
    source = "./modules/database_server"

    name = "database"
    location = "Helsinki"
    server_type = "cx21"
    image = "ubuntu-22.04"
    ssh_keys = [ data.hcloud_ssh_key.ssh_key.name ]
    network_id = hcloud_network.network.id
    volume_size = 40
}

resource "hcloud_load_balancer" "load_balancer" {
    name = "load_balancer"
    load_balancer_type = "lb11"
    location = "Helsinki"
}

resource "hcloud_load_balancer_target" "load_balancer_targer" {
    type = "server"
    load_balancer_id = hcloud_load_balancer.load_balancer.id
    server_id = module.beckend_server.id
}